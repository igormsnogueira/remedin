import { get } from '@/shared/api/client'

export function fetchLocation(signal: AbortSignal): Promise<{ state: string | null }> {
  return get<{ state: string | null }>('/localizacao', signal)
}
