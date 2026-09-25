import { Link } from 'react-router-dom'

import type { MedicineSummary } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { Tag } from '@/shared/ui/Tag/Tag'

import styles from './ResultCard.module.css'

export function ResultCard({ medicine }: { medicine: MedicineSummary }) {
  return (
    <Link className={styles.card} to={`/medicamento/${medicine.registrationNumber}`}>
      <div className={styles.layout}>
        <div>
          <p className={styles.name}>{medicine.name}</p>

          {medicine.manufacturer && <p className={styles.manufacturer}>{medicine.manufacturer}</p>}

          {medicine.purpose && <p className={styles.purpose}>{medicine.purpose}</p>}

          <div className={styles.tags}>
            {medicine.activeIngredient && <Tag>{medicine.activeIngredient}</Tag>}
            {!medicine.isActive && <Tag tone="muted">Registro inativo</Tag>}
          </div>
        </div>

        <div className={styles.price}>
          {medicine.cheapestConsumerPrice === null ? (
            <span className={styles.priceLabel}>Preço não publicado</span>
          ) : (
            <>
              <span className={`${styles.amount} numeric`}>
                {formatPrice(medicine.cheapestConsumerPrice)}
              </span>
              <span className={styles.priceLabel}>preço máximo</span>
            </>
          )}
        </div>
      </div>
    </Link>
  )
}
