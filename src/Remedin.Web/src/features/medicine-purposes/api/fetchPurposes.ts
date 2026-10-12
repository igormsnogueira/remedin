import { get, query } from '@/shared/api/client'
import type { PurposeMedicines, PurposeSummary } from '@/shared/api/types'

export function fetchPurposes(signal: AbortSignal): Promise<PurposeSummary[]> {
  return get<PurposeSummary[]>('/finalidades', signal)
}

export function fetchPurposeMedicines(
  code: string,
  state: string,
  pageNumber: number,
  signal: AbortSignal,
): Promise<PurposeMedicines> {
  return get<PurposeMedicines>(
    `/finalidades/${code}${query({ uf: state, pagina: pageNumber })}`,
    signal,
  )
}
