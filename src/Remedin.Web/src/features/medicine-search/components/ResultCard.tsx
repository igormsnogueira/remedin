import { Link } from 'react-router-dom'

import { FavoriteButton } from '@/features/favorites/components/FavoriteButton'
import { PurposeIcon } from '@/features/medicine-purposes/components/PurposeIcon'
import { purposeStyle } from '@/features/medicine-purposes/model/purposeTheme'
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
    <article
      className={`purpose-theme ${styles.card}`}
      style={purposeStyle(medicine.therapeuticClassCode)}
    >
      <div className={styles.layout}>
        <div>
          {/* Mesmo nome de transição do título da ficha: ao abrir, o nome
              desliza do cartão até o topo da página. */}
          <Link
            className={styles.name}
            to={`/medicamento/${medicine.registrationNumber}`}
            style={{ viewTransitionName: `medicine-${medicine.registrationNumber}` }}
            viewTransition
          >
            {medicine.name}
          </Link>

          {medicine.manufacturer && <p className={styles.manufacturer}>{medicine.manufacturer}</p>}

          {medicine.purpose && (
            <p className={styles.purpose}>
              <PurposeIcon code={medicine.therapeuticClassCode} size="sm" />
              {medicine.purpose}
            </p>
          )}

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
