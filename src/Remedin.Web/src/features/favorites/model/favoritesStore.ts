export interface Favorite {
  registrationNumber: string
  name: string
  manufacturer: string | null
  activeIngredient: string | null
  savedAt: string
}

export type FavoriteInput = Omit<Favorite, 'savedAt'>

// Só no navegador, sem conta: lista de remédios ligada a uma pessoa é dado de saúde (LGPD).
const STORAGE_KEY = 'remedin:favorites'

export const FAVORITES_LIMIT = 100

const EMPTY: Favorite[] = []

const listeners = new Set<() => void>()

// O useSyncExternalStore exige a mesma referência enquanto nada mudou.
let cachedRaw: string | null = null
let cached: Favorite[] = EMPTY

let memoryOnly = false

export function getFavorites(): Favorite[] {
  const raw = readRaw()

  if (raw !== cachedRaw) {
    cachedRaw = raw
    cached = parse(raw)
  }

  return cached
}

export function getServerFavorites(): Favorite[] {
  return EMPTY
}

export function subscribe(listener: () => void): () => void {
  listeners.add(listener)

  function onStorage(event: StorageEvent) {
    if (event.key === STORAGE_KEY || event.key === null) {
      listener()
    }
  }

  window.addEventListener('storage', onStorage)

  return () => {
    listeners.delete(listener)
    window.removeEventListener('storage', onStorage)
  }
}

export function addFavorite(input: FavoriteInput): void {
  const current = getFavorites()

  if (current.length >= FAVORITES_LIMIT || contains(current, input.registrationNumber)) {
    return
  }

  write([{ ...input, savedAt: new Date().toISOString() }, ...current])
}

export function removeFavorite(registrationNumber: string): void {
  write(getFavorites().filter((favorite) => favorite.registrationNumber !== registrationNumber))
}

export function contains(favorites: Favorite[], registrationNumber: string): boolean {
  return favorites.some((favorite) => favorite.registrationNumber === registrationNumber)
}

function readRaw(): string | null {
  if (memoryOnly) {
    return cachedRaw
  }

  try {
    return localStorage.getItem(STORAGE_KEY)
  } catch {
    memoryOnly = true
    return cachedRaw
  }
}

function write(favorites: Favorite[]): void {
  const raw = JSON.stringify(favorites)

  if (!memoryOnly) {
    try {
      localStorage.setItem(STORAGE_KEY, raw)
    } catch {
      memoryOnly = true
    }
  }

  cachedRaw = raw
  cached = favorites
  listeners.forEach((listener) => listener())
}

function parse(raw: string | null): Favorite[] {
  if (!raw) {
    return EMPTY
  }

  try {
    const value: unknown = JSON.parse(raw)
    return Array.isArray(value) ? value.filter(isFavorite) : EMPTY
  } catch {
    return EMPTY
  }
}

function isFavorite(value: unknown): value is Favorite {
  if (typeof value !== 'object' || value === null) {
    return false
  }

  const item = value as Record<string, unknown>

  return (
    typeof item.registrationNumber === 'string' &&
    typeof item.name === 'string' &&
    typeof item.savedAt === 'string' &&
    (item.manufacturer === null || typeof item.manufacturer === 'string') &&
    (item.activeIngredient === null || typeof item.activeIngredient === 'string')
  )
}
