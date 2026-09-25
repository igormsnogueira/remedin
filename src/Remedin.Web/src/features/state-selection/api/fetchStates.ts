import { get } from '@/shared/api/client'
import type { StateOption } from '@/shared/api/types'

export function fetchStates(signal: AbortSignal): Promise<StateOption[]> {
  return get<StateOption[]>('/estados', signal)
}
