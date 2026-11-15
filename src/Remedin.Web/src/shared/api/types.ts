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
  purpose: string | null
}

export interface PagedResults<T> {
  items: T[]
  total: number
  pageNumber: number
  pageSize: number
  hasNext: boolean
  hasPrevious: boolean
  firstPosition: number
  lastPosition: number
}

export interface SearchResults {
  term: string
  state: string
  icmsRate: number
  medicines: PagedResults<MedicineSummary>
}

export interface PurposeSummary {
  code: string
  label: string
  medicineCount: number
}

export interface PurposeMedicines {
  code: string
  label: string
  state: string
  icmsRate: number
  medicines: PagedResults<MedicineSummary>
}

export interface PresentationDetail {
  ggremCode: string
  description: string
  hospitalOnly: boolean
  soldRecently: boolean
  consumerPrice: number | null
  factoryPrice: number | null
  institutional: boolean
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
  requiresPrescription: boolean | null
  hasPrice: boolean
  soldInPharmacy: boolean
  soldRecently: boolean
  cheapestConsumerPrice: number | null
  leafletPublishedOn: string | null
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
