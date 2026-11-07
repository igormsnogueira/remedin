import { Link } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { fetchMedicine } from '@/features/medicine-detail/api/fetchMedicine'
import { PurposeIcon } from '@/features/medicine-purposes/components/PurposeIcon'
import { purposeStyle } from '@/features/medicine-purposes/model/purposeTheme'
import { ApiError } from '@/shared/api/client'
import { useQuery } from '@/shared/api/useQuery'
import type { QueryState } from '@/shared/api/useQuery'
import type { MedicineDetail } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { Button } from '@/shared/ui/Button/Button'
import { Tag } from '@/shared/ui/Tag/Tag'

import type { Favorite } from '../model/favoritesStore'

import styles from './FavoriteCard.module.css'

interface FavoriteCardProps {
  favorite: Favorite
  onRemove: (registrationNumber: string) => void
}

/**
 * Um remédio salvo, com o preço de hoje.
 *
 * O nome vem do que foi guardado, e aparece na hora. O preço não é guardado:
 * muda todo mês e depende do estado, então vem da API a cada visita.
 *
 * Clicável por inteiro como o cartão da busca: o link fica no nome e cobre o
 * cartão, e o botão de remover fica por cima.
 */
export function FavoriteCard({ favorite, onRemove }: FavoriteCardProps) {
  const { state } = useSelectedState()

  const detail = useQuery<MedicineDetail>(
    (signal) => fetchMedicine(favorite.registrationNumber, state, signal),
    [favorite.registrationNumber, state],
  )

  // A finalidade não é guardada com o favorito: vem junto com o preço, e o
  // cartão ganha a cor quando a resposta chega.
  const loaded = detail.status === 'success' ? detail.data : null

  return (
    <article
      className={`purpose-theme ${styles.card}`}
      style={purposeStyle(loaded?.therapeuticClassCode)}
    >
      <div className={styles.layout}>
        <div className={styles.info}>
          <Link
            className={styles.name}
            to={`/medicamento/${favorite.registrationNumber}`}
            style={{ viewTransitionName: `medicine-${favorite.registrationNumber}` }}
            viewTransition
          >
            {favorite.name}
          </Link>

          {favorite.manufacturer && <p className={styles.manufacturer}>{favorite.manufacturer}</p>}

          {loaded?.purpose && (
            <p className={styles.purpose}>
              <PurposeIcon code={loaded.therapeuticClassCode} size="sm" />
              {loaded.purpose}
            </p>
          )}

          {favorite.activeIngredient && (
            <div className={styles.tags}>
              <Tag>{favorite.activeIngredient}</Tag>
            </div>
          )}
        </div>

        <div className={styles.aside}>
          <Price detail={detail} />

          <Button
            className={styles.remove}
            type="button"
            onClick={() => onRemove(favorite.registrationNumber)}
          >
            Remover
            <span className="visually-hidden"> {favorite.name} dos favoritos</span>
          </Button>
        </div>
      </div>
    </article>
  )
}

function Price({ detail }: { detail: QueryState<MedicineDetail> }) {
  if (detail.status === 'idle' || detail.status === 'loading') {
    // Espaço reservado com a altura do preço, para o cartão não pular.
    return <span className={styles.pricePlaceholder} aria-hidden="true" />
  }

  if (detail.status === 'error') {
    const gone = detail.error instanceof ApiError && detail.error.isNotFound

    return (
      <span className={styles.priceLabel}>
        {gone ? 'Saiu da lista da CMED' : 'Preço indisponível agora'}
      </span>
    )
  }

  if (detail.data.cheapestConsumerPrice === null) {
    return <span className={styles.priceLabel}>Preço não publicado</span>
  }

  return (
    <span className={styles.price}>
      <span className={`${styles.amount} numeric`}>
        {formatPrice(detail.data.cheapestConsumerPrice)}
      </span>
      <span className={styles.priceLabel}>preço máximo em {detail.data.state}</span>
    </span>
  )
}
