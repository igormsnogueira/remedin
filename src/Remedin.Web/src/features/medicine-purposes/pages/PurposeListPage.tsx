import { Link } from 'react-router-dom'

import { ApiLoading } from '@/shared/api/ApiLoading'
import { useQuery } from '@/shared/api/useQuery'
import type { PurposeSummary } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'

import { fetchPurposes } from '../api/fetchPurposes'
import { PurposeIcon } from '../components/PurposeIcon'
import { purposeStyle } from '../model/purposeTheme'

import styles from './PurposeListPage.module.css'

export function PurposeListPage() {
  useDocumentTitle('Medicamentos por finalidade')

  const purposes = useQuery<PurposeSummary[]>(fetchPurposes, [])

  return (
    <div className={styles.page}>
      <div className={styles.heading}>
        <h1 className={styles.title}>Para que você precisa de remédio?</h1>
        <p className={styles.intro}>
          Escolha a área do corpo ou o tipo de problema. É um caminho para quem não sabe o nome do
          medicamento.
        </p>
      </div>

      {purposes.status === 'loading' && <ApiLoading label="Carregando as finalidades" shape="grid" />}

      {purposes.status === 'error' && <Notice tone="danger">{purposes.error.message}</Notice>}

      {purposes.status === 'success' && (
        <div className={styles.grid}>
          {/* Do grupo com mais remédios para o com menos: os primeiros são
              os que mais gente procura, e ganham o bloco maior. */}
          {[...purposes.data]
            .sort((a, b) => b.medicineCount - a.medicineCount)
            .map((purpose, index) => (
              <Link
                className={`purpose-theme ${styles.card} ${index < 2 ? styles.featured : ''}`}
                key={purpose.code}
                to={`/finalidades/${purpose.code}`}
                style={{
                  ...purposeStyle(purpose.code),
                  viewTransitionName: `purpose-${purpose.code}`,
                }}
                viewTransition
              >
                <PurposeIcon code={purpose.code} size={index < 2 ? 'lg' : 'md'} onTint />
                <span className={styles.label}>{purpose.label}</span>
                <span className={`${styles.count} numeric`}>
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
