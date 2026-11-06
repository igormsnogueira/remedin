import { Link } from 'react-router-dom'

import { FavoriteButton } from '@/features/favorites/components/FavoriteButton'
import type { MedicineSummary } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { Tag } from '@/shared/ui/Tag/Tag'

import styles from './ResultCard.module.css'

/**
 * Um resultado de busca, clicável por inteiro.
 *
 * O cartão não é mais um <a> em volta de tudo: o botão de favorito não pode
 * ficar dentro de um link. O link fica no nome e cobre o cartão com um
 * pseudo-elemento, e o botão fica por cima dele.
 */
export function ResultCard({ medicine }: { medicine: MedicineSummary }) {
  return (
    <article className={styles.card}>
      <div className={styles.layout}>
        <div>
          <Link className={styles.name} to={`/medicamento/${medicine.registrationNumber}`}>
            {medicine.name}
          </Link>

          {medicine.manufacturer && <p className={styles.manufacturer}>{medicine.manufacturer}</p>}

          {medicine.purpose && <p className={styles.purpose}>{medicine.purpose}</p>}

          <div className={styles.tags}>
            {medicine.activeIngredient && <Tag>{medicine.activeIngredient}</Tag>}
            {!medicine.isActive && <Tag tone="muted">Registro inativo</Tag>}
          </div>
        </div>

        <div className={styles.aside}>
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

          <FavoriteButton className={styles.favorite} medicine={medicine} compact />
        </div>
      </div>
    </article>
  )
}
