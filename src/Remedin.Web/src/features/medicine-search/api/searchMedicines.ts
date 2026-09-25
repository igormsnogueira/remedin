import { get, query } from '@/shared/api/client'
import type { SearchResults } from '@/shared/api/types'

export function searchMedicines(
  term: string,
  state: string,
  signal: AbortSignal,
): Promise<SearchResults> {
  return get<SearchResults>(`/medicamentos${query({ q: term, uf: state })}`, signal)
}
