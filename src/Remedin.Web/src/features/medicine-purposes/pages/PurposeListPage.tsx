import { Link } from 'react-router-dom'

import { useQuery } from '@/shared/api/useQuery'
import type { PurposeSummary } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Spinner } from '@/shared/ui/Spinner/Spinner'

import { fetchPurposes } from '../api/fetchPurposes'

import styles from './PurposeListPage.module.css'

export function PurposeListPage() {
  useDocumentTitle('Medicamentos por finalidade')

  const purposes = useQuery<PurposeSummary[]>(fetchPurposes, [])

  return (
    <div className={styles.page}>
      <div>
        <h1>Para que você precisa de remédio?</h1>
        <p className={styles.intro}>
          Escolha a área do corpo ou o tipo de problema. É um caminho para quem não sabe o nome do
          medicamento.
        </p>
      </div>

      {purposes.status === 'loading' && <Spinner label="Carregando as finalidades" />}

      {purposes.status === 'error' && <Notice tone="danger">{purposes.error.message}</Notice>}

      {purposes.status === 'success' && (
        <div className={styles.grid}>
          {purposes.data.map((purpose) => (
            <Link className={styles.card} key={purpose.code} to={`/finalidades/${purpose.code}`}>
              <span className={styles.label}>{purpose.label}</span>
              <span className={styles.count}>
                {purpose.medicineCount.toLocaleString('pt-BR')} medicamentos
              </span>
            </Link>
          ))}
        </div>
      )}

      <p>
        <Link to="/">Buscar pelo nome</Link>
      </p>
    </div>
  )
}
