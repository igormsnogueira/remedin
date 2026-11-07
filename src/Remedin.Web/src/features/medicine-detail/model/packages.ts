import type { PresentationDetail } from '@/shared/api/types'
import { describePresentation } from '@/shared/format/presentation'

/** Uma embalagem como a pessoa escolhe: o que vem, quanto custa. */
export interface PackageOption {
  /** Código GGREM da primeira embalagem do grupo. Vai na URL. */
  key: string
  title: string
  price: number | null
  /** "10 mg", "4 mg/ml". Nula quando a descrição não começa pela dosagem. */
  strength: string | null
  milligrams: number | null
  units: { count: number; name: string } | null
  release: 'prolonged' | 'delayed' | 'normal' | null
  /**
   * Preço de uma unidade: um comprimido, um ml, um grama. É o que permite
   * comparar o frasco de 10 ml com o de 20 ml.
   */
  perUnit: number | null
  /** "comprimido", "ml", "g". Nulo quando não dá para dividir. */
  unitLabel: string | null
  /** Só vale mostrar o preço por unidade quando a embalagem tem mais de uma. */
  showPerUnit: boolean
}

/**
 * Embalagens que só diferem no material do blister saem com o mesmo texto
 * legível e o mesmo preço. Para quem compra são a mesma coisa, então viram uma
 * opção só. Ordem: da mais barata para a mais cara, sem preço por último.
 */
export function groupPackages(
  presentations: PresentationDetail[],
  hospital: boolean,
): PackageOption[] {
  const options = new Map<string, PackageOption>()

  for (const presentation of presentations) {
    if (presentation.hospitalOnly !== hospital) {
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
