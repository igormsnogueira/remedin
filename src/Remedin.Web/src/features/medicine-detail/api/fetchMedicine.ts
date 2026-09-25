import { get, query } from '@/shared/api/client'
import type { MedicineDetail } from '@/shared/api/types'

export function fetchMedicine(
  registrationNumber: string,
  state: string,
  signal: AbortSignal,
): Promise<MedicineDetail> {
  return get<MedicineDetail>(
    `/medicamentos/${registrationNumber}${query({ uf: state })}`,
    signal,
  )
}
