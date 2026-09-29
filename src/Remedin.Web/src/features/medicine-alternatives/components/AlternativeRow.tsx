import { Link } from 'react-router-dom'

import type { MedicineAlternative } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { classNames } from '@/shared/ui/classNames'
import { Tag } from '@/shared/ui/Tag/Tag'

import styles from './AlternativeRow.module.css'

interface AlternativeRowProps {
  alternative: MedicineAlternative
  /** Dosagem do medicamento consultado, para apontar as que não batem. */
  referenceDosage: number | null
}

export function AlternativeRow({ alternative, referenceDosage }: AlternativeRowProps) {
  const differentDosage =
    referenceDosage !== null &&
    alternative.dosageInMilligrams !== null &&
    alternative.dosageInMilligrams !== referenceDosage

  return (
    <li className={classNames(styles.row, alternative.isCurrent && styles.current)}>
      <div>
        {alternative.isCurrent ? (
          <p className={styles.name}>{alternative.name}</p>
        ) : (
          <Link className={styles.name} to={`/medicamento/${alternative.registrationNumber}`}>
            {alternative.name}
          </Link>
        )}

        {alternative.manufacturer && (
          <p className={styles.manufacturer}>{alternative.manufacturer}</p>
        )}

        <p className={styles.presentation}>{alternative.presentation}</p>

        <div className={styles.tags}>
          {alternative.isCurrent && <Tag>Você está vendo este</Tag>}

          {/* A dosagem aparece sempre que é conhecida. Mostrar só quando
              diverge esconderia a informação justamente quando a dosagem do
              medicamento consultado não pôde ser lida, que é quando a pessoa
              mais precisa comparar por conta própria. */}
          {alternative.dosageInMilligrams !== null && (
            <Tag tone="muted">
              {differentDosage && 'Dosagem diferente: '}
              {alternative.dosageInMilligrams} MG
            </Tag>
          )}
        </div>
      </div>

      <div className={styles.price}>
        {alternative.pricePerUnit === null ? (
          <>
            <span className={`${styles.box} numeric`}>{formatPrice(alternative.consumerPrice)}</span>
            <span className={styles.unit}>a embalagem inteira</span>
          </>
        ) : (
          <>
            <span className={`${styles.amount} numeric`}>
              {formatPrice(alternative.pricePerUnit)}
            </span>
            <span className={styles.unit}>
              por unidade · {formatPrice(alternative.consumerPrice)} a caixa
            </span>
          </>
        )}
      </div>
    </li>
  )
}
