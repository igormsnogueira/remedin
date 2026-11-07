import { useSelectedState } from '@/app/providers/useSelectedState'
import { fetchAlternatives } from '@/features/medicine-alternatives/api/fetchAlternatives'
import { fetchMedicine } from '@/features/medicine-detail/api/fetchMedicine'
import { groupPackages } from '@/features/medicine-detail/model/packages'
import { useQuery } from '@/shared/api/useQuery'
import type { AlternativesResult, MedicineAlternative, MedicineDetail } from '@/shared/api/types'
import { describePresentation } from '@/shared/format/presentation'

/**
 * Omeprazol 20 mg, cápsulas. Escolhido porque a diferença entre marcas é
 * grande, a dosagem é a mesma nos dois lados e a contagem de unidades é
 * legível — a comparação é honesta (ADR 0011). Se o registro sair da lista da
 * CMED, os blocos que dependem dele somem.
 */
export const EXAMPLE_REGISTRATION = '102980354'

/** O exemplo já mastigado para a tela. Nulo enquanto carrega ou se falhar. */
export interface Example {
  medicine: MedicineDetail
  /** Preço da embalagem mais barata vendida em farmácia. */
  lowestPrice: number | null
  /** Nulo quando não há equivalente comparável ou a diferença é pequena. */
  comparison: {
    current: MedicineAlternative & { pricePerUnit: number }
    cheapest: MedicineAlternative & { pricePerUnit: number }
    ratio: number
    unitName: string
  } | null
  /** Outros produtos com o mesmo princípio ativo, para a cena da busca. */
  others: MedicineAlternative[]
}

export type ExampleState = { status: 'loading' } | { status: 'error' } | { status: 'ready'; example: Example }

/**
 * Abaixo do dobro, "1,3 vezes" não impressiona e só ocupa espaço.
 */
const MIN_RATIO = 2

/**
 * Consulta o exemplo uma vez para a página toda: o topo e a história da
 * rolagem mostram o mesmo remédio, e duas chamadas iguais seriam desperdício.
 *
 * Nenhum número fica escrito na página: o preço muda todo mês, e um valor fixo
 * ficaria falso sem ninguém perceber. As chamadas também acordam a API
 * enquanto a pessoa ainda está lendo.
 */
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
    groupPackages(medicine.data.presentations, false).find((option) => option.price !== null)
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

/**
 * Genéricos têm o mesmo nome, e "OMEPRAZOL" contra "OMEPRAZOL" não diz nada.
 * A primeira palavra do laboratório basta para distinguir: "EMS", "SANOFI".
 */
export function withMaker(alternative: { name: string; manufacturer: string | null }): string {
  const maker = alternative.manufacturer?.split(' ')[0]
  return maker ? `${alternative.name} (${maker})` : alternative.name
}
