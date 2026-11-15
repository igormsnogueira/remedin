import { useSelectedState } from '@/app/providers/useSelectedState'
import { fetchAlternatives } from '@/features/medicine-alternatives/api/fetchAlternatives'
import { fetchMedicine } from '@/features/medicine-detail/api/fetchMedicine'
import { groupPackages } from '@/features/medicine-detail/model/packages'
import { useQuery } from '@/shared/api/useQuery'
import type { AlternativesResult, MedicineAlternative, MedicineDetail } from '@/shared/api/types'
import { describePresentation } from '@/shared/format/presentation'

export const EXAMPLE_REGISTRATION = '102980354'

export interface Example {
  medicine: MedicineDetail
  lowestPrice: number | null
  comparison: {
    current: MedicineAlternative & { pricePerUnit: number }
    cheapest: MedicineAlternative & { pricePerUnit: number }
    ratio: number
    unitName: string
  } | null
  others: MedicineAlternative[]
}

export type ExampleState = { status: 'loading' } | { status: 'error' } | { status: 'ready'; example: Example }

const MIN_RATIO = 2

export function useExample(): ExampleState {
  const { state } = useSelectedState()

  const medicine = useQuery<MedicineDetail>(
    (signal) => fetchMedicine(EXAMPLE_REGISTRATION, state, signal),
    [EXAMPLE_REGISTRATION, state],
  )
  const alternatives = useQuery<AlternativesResult>(
    (signal) => fetchAlternatives(EXAMPLE_REGISTRATION, state, signal),
    [EXAMPLE_REGISTRATION, state],
  )

  if (medicine.status === 'error') {
    return { status: 'error' }
  }

  if (medicine.status !== 'success') {
    return { status: 'loading' }
  }

  const lowestPrice =
    groupPackages(medicine.data.presentations, 'pharmacy').find((option) => option.price !== null)
      ?.price ?? null

  const result = alternatives.status === 'success' ? alternatives.data : null

  return {
    status: 'ready',
    example: {
      medicine: medicine.data,
      lowestPrice,
      comparison: result ? compare(result) : null,
      others: result?.alternatives.filter((alternative) => !alternative.isCurrent) ?? [],
    },
  }
}

function compare(result: AlternativesResult): Example['comparison'] {
  const current = result.alternatives.find((alternative) => alternative.isCurrent)
  const cheapest = result.cheapestComparable

  if (!current?.pricePerUnit || !cheapest?.pricePerUnit) {
    return null
  }

  const ratio = current.pricePerUnit / cheapest.pricePerUnit

  if (ratio < MIN_RATIO) {
    return null
  }

  return {
    current: { ...current, pricePerUnit: current.pricePerUnit },
    cheapest: { ...cheapest, pricePerUnit: cheapest.pricePerUnit },
    ratio,
    unitName: describePresentation(current.presentation)?.units?.name ?? 'unidade',
  }
}

export function withMaker(alternative: { name: string; manufacturer: string | null }): string {
  const maker = alternative.manufacturer?.split(' ')[0]
  return maker ? `${alternative.name} (${maker})` : alternative.name
}
