import { get, query } from '@/shared/api/client'
import type { SearchResults } from '@/shared/api/types'

/**
 * Quantos resultados pedir.
 *
 * Declarado aqui e enviado na requisição, em vez de contar com o padrão da API:
 * a tela avisa quando a lista está cortada, e para isso precisa saber o número
 * que pediu.
 */
export const SEARCH_LIMIT = 20

export function searchMedicines(
  term: string,
  state: string,
  signal: AbortSignal,
): Promise<SearchResults> {
  return get<SearchResults>(
    `/medicamentos${query({ q: term, uf: state, limite: SEARCH_LIMIT })}`,
    signal,
  )
}
