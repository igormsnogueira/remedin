import { Link, useParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
// O cartão vem da busca em vez de shared/ui: ele conhece o formato de
// MedicineSummary e a rota do medicamento, então não é primitivo de interface.
// Se uma terceira tela precisar dele, aí sim vira componente compartilhado.
import { ResultCard } from '@/features/medicine-search/components/ResultCard'
import { StateSelect } from '@/features/state-selection/components/StateSelect'
import { ApiError } from '@/shared/api/client'
import { useQuery } from '@/shared/api/useQuery'
import type { PurposeMedicines } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Spinner } from '@/shared/ui/Spinner/Spinner'

import { PURPOSE_LIMIT, fetchPurposeMedicines } from '../api/fetchPurposes'

import styles from './PurposeMedicinesPage.module.css'

export function PurposeMedicinesPage() {
  const { code } = useParams()
  const { state } = useSelectedState()

  const purpose = useQuery<PurposeMedicines>(
    code ? (signal) => fetchPurposeMedicines(code, state, signal) : null,
    [code, state],
  )

  useDocumentTitle(purpose.status === 'success' ? purpose.data.label : null)

  if (purpose.status === 'loading' || purpose.status === 'idle') {
    return <Spinner label="Carregando os medicamentos" />
  }

  if (purpose.status === 'error') {
    const notFound = purpose.error instanceof ApiError && purpose.error.isNotFound

    return (
      <div className={styles.page}>
        <Notice tone="danger">
          {notFound ? 'Finalidade não encontrada.' : purpose.error.message}
        </Notice>
        <Link to="/finalidades">Ver todas as finalidades</Link>
      </div>
    )
  }

  const { label, medicines, state: uf } = purpose.data

  return (
    <div className={styles.page}>
      <div className={styles.heading}>
        <h1>{label}</h1>
        <StateSelect className={styles.state} />
      </div>

      {medicines.length === 0 ? (
        <Notice>Nenhum medicamento desta finalidade tem preço publicado em {uf}.</Notice>
      ) : (
        <section className={styles.list}>
          <p className={styles.count}>
            Do mais barato ao mais caro · preço para {uf}
          </p>

          {medicines.map((medicine) => (
            <ResultCard key={medicine.registrationNumber} medicine={medicine} />
          ))}

          {medicines.length === PURPOSE_LIMIT && (
            <Notice>
              A lista mostra os {PURPOSE_LIMIT} mais baratos. Para ver outros, busque pelo nome do
              medicamento.
            </Notice>
          )}
        </section>
      )}

      <p>
        <Link to="/finalidades">Ver todas as finalidades</Link>
      </p>
    </div>
  )
}
