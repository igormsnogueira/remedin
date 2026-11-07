import { Link } from 'react-router-dom'

import type { MedicineAlternative } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { describePresentation } from '@/shared/format/presentation'

import styles from './AlternativeRow.module.css'

/** Uma linha da lista de equivalentes, clicável por inteiro. */
export function AlternativeRow({ alternative }: { alternative: MedicineAlternative }) {
  const readable = describePresentation(alternative.presentation)
  const measure = readable?.measure ?? null
  const perMeasure =
    alternative.pricePerUnit === null && measure && measure.amount > 1
      ? alternative.consumerPrice / measure.amount
      : null
  const presentation = readable
    ? [readable.content, readable.strength].filter(Boolean).join(' · ')
    : alternative.presentation

  return (
    <li className={styles.row}>
      <div className={styles.info}>
        <Link
          className={styles.name}
          to={`/medicamento/${alternative.registrationNumber}`}
          style={{ viewTransitionName: `medicine-${alternative.registrationNumber}` }}
          viewTransition
        >
          {alternative.name}
        </Link>

        <p className={styles.meta}>
          {[alternative.manufacturer, presentation].filter(Boolean).join(' · ')}
        </p>
      </div>

      <div className={styles.price}>
        {perMeasure !== null && measure ? (
          <>
            <span className={`${styles.amount} numeric`}>{formatPrice(perMeasure)}</span>
            <span className={styles.unit}>
              por {measure.unit} ·{' '}
              <span className="numeric">{formatPrice(alternative.consumerPrice)}</span> a embalagem
            </span>
          </>
        ) : alternative.pricePerUnit === null ? (
          <>
            <span className={`${styles.amount} numeric`}>
              {formatPrice(alternative.consumerPrice)}
            </span>
            <span className={styles.unit}>a embalagem</span>
          </>
        ) : (
          <>
            <span className={`${styles.amount} numeric`}>
              {formatPrice(alternative.pricePerUnit)}
            </span>
            <span className={styles.unit}>
              por unidade · <span className="numeric">{formatPrice(alternative.consumerPrice)}</span> a
              caixa
            </span>
          </>
        )}
      </div>
    </li>
  )
}
