/**
 * A lista de favoritos, guardada no navegador da pessoa.
 *
 * Fica no localStorage, e não na API, de propósito: guardar no servidor
 * exigiria conta e login, e uma lista de remédios ligada a uma pessoa é dado
 * de saúde, que a LGPD trata como sensível. Aqui nada sai do aparelho.
 *
 * Fica fora do React, num módulo, e os componentes leem com
 * useSyncExternalStore. Assim o botão da ficha, o cartão da busca e o contador
 * do cabeçalho mostram sempre a mesma lista, inclusive entre abas abertas.
 */

export interface Favorite {
  registrationNumber: string
  name: string
  manufacturer: string | null
  activeIngredient: string | null
  /** ISO 8601. A lista sai do mais recente para o mais antigo. */
  savedAt: string
}

export type FavoriteInput = Omit<Favorite, 'savedAt'>

const STORAGE_KEY = 'remedin:favorites'

/** Teto para a lista não crescer sem fim no armazenamento do navegador. */
export const FAVORITES_LIMIT = 100

const EMPTY: Favorite[] = []

const listeners = new Set<() => void>()

// A lista é lida do armazenamento a cada renderização, mas só é convertida de
// novo quando o texto guardado muda. O useSyncExternalStore exige a mesma
// referência enquanto nada mudou, senão renderiza em laço.
let cachedRaw: string | null = null
let cached: Favorite[] = EMPTY

// Com o armazenamento bloqueado (navegação anônima em alguns navegadores, ou
// cota cheia), a lista passa a viver só na memória até a aba fechar.
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

  // O evento storage só chega nas outras abas, nunca na que gravou. É ele que
  // mantém duas abas abertas com a mesma lista.
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

/**
 * O conteúdo do localStorage pode ter sido editado à mão ou gravado por uma
 * versão antiga do site. Item que não tem o formato esperado é descartado, em
 * vez de quebrar a página.
 */
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
