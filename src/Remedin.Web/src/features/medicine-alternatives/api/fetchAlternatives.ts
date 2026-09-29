import { get, query } from '@/shared/api/client'
import type { AlternativesResult } from '@/shared/api/types'

export function fetchAlternatives(
  registrationNumber: string,
  state: string,
  signal: AbortSignal,
): Promise<AlternativesResult> {
  return get<AlternativesResult>(
    `/medicamentos/${registrationNumber}/alternativas${query({ uf: state })}`,
    signal,
  )
}
