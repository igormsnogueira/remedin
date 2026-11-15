import { classNames } from '@/shared/ui/classNames'

import { FAVORITES_LIMIT } from '../model/favoritesStore'
import type { FavoriteInput } from '../model/favoritesStore'
import { useFavorites } from '../model/useFavorites'

import styles from './FavoriteButton.module.css'

interface FavoriteButtonProps {
  medicine: FavoriteInput
  compact?: boolean
  className?: string
}

export function FavoriteButton({ medicine, compact = false, className }: FavoriteButtonProps) {
  const { isFavorite, isFull, add, remove } = useFavorites()

  const saved = isFavorite(medicine.registrationNumber)
  const blocked = !saved && isFull

  function toggle() {
    if (saved) {
      remove(medicine.registrationNumber)
    } else {
      add({
        registrationNumber: medicine.registrationNumber,
        name: medicine.name,
        manufacturer: medicine.manufacturer,
        activeIngredient: medicine.activeIngredient,
      })
    }
  }

  return (
    <button
      type="button"
      className={classNames(styles.button, compact && styles.compact, className)}
      aria-pressed={saved}
      aria-label={compact ? `Favorito: ${medicine.name}` : undefined}
      title={blocked ? `Limite de ${FAVORITES_LIMIT} favoritos atingido` : undefined}
      disabled={blocked}
      onClick={toggle}
    >
      <svg className={styles.icon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
        <path
          d="M6.5 3.5h11v17l-5.5-4-5.5 4z"
          fill={saved ? 'currentColor' : 'none'}
          stroke="currentColor"
          strokeWidth="2"
          strokeLinejoin="round"
        />
      </svg>
      {!compact && 'Favorito'}
    </button>
  )
}
