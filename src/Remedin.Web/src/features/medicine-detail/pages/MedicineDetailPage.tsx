import { Link, useParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { ApiError } from '@/shared/api/client'
import { useQuery } from '@/shared/api/useQuery'
import type { MedicineDetail } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Spinner } from '@/shared/ui/Spinner/Spinner'
import { Tag } from '@/shared/ui/Tag/Tag'

import { fetchMedicine } from '../api/fetchMedicine'

import styles from './MedicineDetailPage.module.css'

export function MedicineDetailPage() {
  const { registrationNumber } = useParams()
  const { state } = useSelectedState()

  const medicine = useQuery<MedicineDetail>(
    registrationNumber
      ? (signal) => fetchMedicine(registrationNumber, state, signal)
      : null,
    [registrationNumber, state],
  )

  if (medicine.status === 'loading' || medicine.status === 'idle') {
    return <Spinner label="Carregando o medicamento" />
  }

  if (medicine.status === 'error') {
    const notFound = medicine.error instanceof ApiError && medicine.error.isNotFound

    return (
      <div className={styles.page}>
        <Notice tone="danger">
          {notFound ? 'Medicamento não encontrado.' : medicine.error.message}
        </Notice>
        <Link to="/">Voltar para a busca</Link>
      </div>
    )
  }

  const data = medicine.data
  const onSale = data.presentations.filter((presentation) => !presentation.hospitalOnly)

  return (
    <article className={styles.page}>
      <header className={styles.header}>
        <h1>{data.name}</h1>

        {data.manufacturer && <p className={styles.manufacturer}>{data.manufacturer}</p>}

        <div className={styles.tags}>
          {data.activeIngredient && <Tag>{data.activeIngredient}</Tag>}
          {data.prescriptionRule && <Tag tone="muted">{data.prescriptionRule}</Tag>}
          {!data.isActive && <Tag tone="muted">Registro inativo</Tag>}
        </div>
      </header>

      {data.purpose && (
        <section>
          <h2>Para que serve</h2>
          <p>{data.purpose}</p>
        </section>
      )}

      <section>
        <h2>Preço máximo em {data.state}</h2>

        {onSale.length === 0 ? (
          <Notice tone="warning">
            Este medicamento não tem apresentação de venda em farmácia com preço publicado.
          </Notice>
        ) : (
          <div className={styles.presentations}>
            {onSale.map((presentation) => (
              <div className={styles.presentation} key={presentation.ggremCode}>
                <span className={styles.description}>{presentation.description}</span>
                <span className={`${styles.amount} numeric`}>
                  {presentation.consumerPrice === null
                    ? 'sem preço publicado'
                    : formatPrice(presentation.consumerPrice)}
                </span>
              </div>
            ))}
          </div>
        )}
      </section>

      <Link to="/">Voltar para a busca</Link>
    </article>
  )
}
