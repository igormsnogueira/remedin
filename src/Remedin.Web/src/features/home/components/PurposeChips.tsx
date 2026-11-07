import { Link } from 'react-router-dom'

import { fetchPurposes } from '@/features/medicine-purposes/api/fetchPurposes'
import { PurposeIcon } from '@/features/medicine-purposes/components/PurposeIcon'
import { useQuery } from '@/shared/api/useQuery'
import type { PurposeSummary } from '@/shared/api/types'

import styles from './PurposeChips.module.css'

/** Quantas finalidades aparecem. As outras ficam a um clique. */
const SHOWN = 6

/**
 * A entrada pela finalidade, logo abaixo da busca: as que têm mais
 * medicamentos, com a contagem vinda da API. Se a chamada falhar, fica só o
 * link para a lista completa.
 */
export function PurposeChips() {
  const purposes = useQuery<PurposeSummary[]>(fetchPurposes, [])

  const shown =
    purposes.status === 'success'
      ? [...purposes.data].sort((a, b) => b.medicineCount - a.medicineCount).slice(0, SHOWN)
      : []

  return (
    <nav className={styles.chips} aria-labelledby="chips-title">
      <p className={styles.label} id="chips-title">
        Ou escolha pela finalidade
      </p>

      <ul className={styles.list} aria-busy={purposes.status === 'loading'}>
        {/* Formato dos atalhos enquanto a lista não chega, para a faixa não
            mudar de altura quando eles aparecem. */}
        {purposes.status === 'loading' &&
          Array.from({ length: SHOWN }, (_, index) => (
            <li key={index} className={styles.ghost} aria-hidden="true" />
          ))}

        {shown.map((purpose) => (
          <li key={purpose.code}>
            <Link className={styles.chip} to={`/finalidades/${purpose.code}`} viewTransition>
              <PurposeIcon code={purpose.code} size="sm" />
              {purpose.label}
            </Link>
          </li>
        ))}
        <li>
          <Link className={styles.all} to="/finalidades">
            Ver todas
          </Link>
        </li>
      </ul>
    </nav>
  )
}
