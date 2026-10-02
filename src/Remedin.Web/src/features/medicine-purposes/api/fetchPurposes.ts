import { get, query } from '@/shared/api/client'
import type { PurposeMedicines, PurposeSummary } from '@/shared/api/types'

/** Mesmo teto da busca, pelo mesmo motivo: lista que ninguém percorre inteira. */
export const PURPOSE_LIMIT = 20

export function fetchPurposes(signal: AbortSignal): Promise<PurposeSummary[]> {
  return get<PurposeSummary[]>('/finalidades', signal)
}

export function fetchPurposeMedicines(
  code: string,
  state: string,
  signal: AbortSignal,
): Promise<PurposeMedicines> {
  return get<PurposeMedicines>(
    `/finalidades/${code}${query({ uf: state, limite: PURPOSE_LIMIT })}`,
    signal,
  )
}
