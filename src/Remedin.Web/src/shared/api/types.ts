/**
 * O formato das respostas da API.
 *
 * Escrito à mão e não gerado: são poucos endpoints, e um gerador de tipos a
 * partir do OpenAPI é mais uma ferramenta no build para manter. Se a API
 * crescer, a decisão se reabre.
 *
 * Campo opcional aqui é campo que a fonte pode não ter — ausência de dado é
 * parte do domínio, não erro de digitação.
 */

export interface StateOption {
  code: string
  name: string
  icmsRate: number
}

export interface MedicineSummary {
  registrationNumber: string
  name: string
  activeIngredient: string | null
  manufacturer: string | null
  therapeuticClassCode: string | null
  therapeuticClass: string | null
  isActive: boolean
  cheapestConsumerPrice: number | null
  /** Para que serve, em linguagem comum. Nulo quando não há tradução. */
  purpose: string | null
}

export interface SearchResults {
  term: string
  state: string
  icmsRate: number
  medicines: MedicineSummary[]
}

export interface PresentationDetail {
  ggremCode: string
  description: string
  hospitalOnly: boolean
  soldRecently: boolean
  consumerPrice: number | null
  factoryPrice: number | null
}

export interface MedicineDetail {
  registrationNumber: string
  name: string
  activeIngredient: string | null
  manufacturer: string | null
  therapeuticClassCode: string | null
  therapeuticClassName: string | null
  prescriptionBand: string | null
  isActive: boolean
  state: string
  icmsRate: number
  presentations: PresentationDetail[]
  purpose: string | null
  prescriptionRule: string | null
  /** Nulo quando a fonte não informa, que é diferente de não exigir. */
  requiresPrescription: boolean | null
  hasPrice: boolean
  soldInPharmacy: boolean
  soldRecently: boolean
  cheapestConsumerPrice: number | null
}

export interface MedicineAlternative {
  registrationNumber: string
  name: string
  manufacturer: string | null
  presentation: string
  consumerPrice: number
  dosageInMilligrams: number | null
  unitCount: number | null
  isCurrent: boolean
  /** Nulo quando a embalagem não permite ler a quantidade. */
  pricePerUnit: number | null
}

export interface AlternativesResult {
  registrationNumber: string
  activeIngredient: string | null
  state: string
  icmsRate: number
  alternatives: MedicineAlternative[]
  cheapestComparable: MedicineAlternative | null
  savingsPerUnit: number | null
  notice: string
}
