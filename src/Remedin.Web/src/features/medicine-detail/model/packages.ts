import type { PresentationDetail } from '@/shared/api/types'
import { describePresentation } from '@/shared/format/presentation'

export interface PackageOption {
  key: string
  title: string
  price: number | null
  strength: string | null
  milligrams: number | null
  units: { count: number; name: string } | null
  release: 'prolonged' | 'delayed' | 'normal' | null
  perUnit: number | null
  unitLabel: string | null
  showPerUnit: boolean
}

export type PackageKind = 'pharmacy' | 'institutional' | 'hospital'

function kindOf(presentation: PresentationDetail): PackageKind {
  if (presentation.hospitalOnly) {
    return 'hospital'
  }

  return presentation.institutional ? 'institutional' : 'pharmacy'
}

export function groupPackages(
  presentations: PresentationDetail[],
  kind: PackageKind,
): PackageOption[] {
  const options = new Map<string, PackageOption>()
  const hospital = kind === 'hospital'

  for (const presentation of presentations) {
    if (kindOf(presentation) !== kind) {
      continue
    }

    const readable = describePresentation(presentation.description)
    const title = readable
      ? [readable.content, readable.strength].filter(Boolean).join(' · ')
      : presentation.description
    const price = hospital ? presentation.factoryPrice : presentation.consumerPrice
    const group = `${title}|${price}`

    if (options.has(group)) {
      continue
    }

    const units = readable?.units ?? null
    const measure = readable?.measure ?? null
    const divisor = units?.count ?? measure?.amount ?? null

    options.set(group, {
      key: presentation.ggremCode,
      title,
      price,
      strength: readable?.strength ?? null,
      milligrams: readable?.milligrams ?? null,
      units,
      release: readable?.release ?? null,
      perUnit: price !== null && divisor ? price / divisor : null,
      unitLabel: units?.name ?? measure?.unit ?? null,
      showPerUnit: divisor !== null && divisor > 1,
    })
  }

  return [...options.values()].sort(
    (a, b) => (a.price ?? Number.POSITIVE_INFINITY) - (b.price ?? Number.POSITIVE_INFINITY),
  )
}
