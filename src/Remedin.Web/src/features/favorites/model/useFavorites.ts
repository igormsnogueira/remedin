import { useSyncExternalStore } from 'react'

import {
  addFavorite,
  contains,
  FAVORITES_LIMIT,
  getFavorites,
  getServerFavorites,
  removeFavorite,
  subscribe,
} from './favoritesStore'
import type { Favorite, FavoriteInput } from './favoritesStore'

interface FavoritesApi {
  favorites: Favorite[]
  isFavorite: (registrationNumber: string) => boolean
  isFull: boolean
  add: (input: FavoriteInput) => void
  remove: (registrationNumber: string) => void
}

export function useFavorites(): FavoritesApi {
  const favorites = useSyncExternalStore(subscribe, getFavorites, getServerFavorites)

  return {
    favorites,
    isFavorite: (registrationNumber) => contains(favorites, registrationNumber),
    isFull: favorites.length >= FAVORITES_LIMIT,
    add: addFavorite,
    remove: removeFavorite,
  }
}
