import { get } from '@/shared/api/client'

/** O estado provável de quem acessa, estimado pela API a partir do IP. */
export function fetchLocation(signal: AbortSignal): Promise<{ state: string | null }> {
  return get<{ state: string | null }>('/localizacao', signal)
}
