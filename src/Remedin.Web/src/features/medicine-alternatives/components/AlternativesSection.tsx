import { useState } from 'react'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { ApiLoading } from '@/shared/api/ApiLoading'
import { useQuery } from '@/shared/api/useQuery'
import type { AlternativesResult, MedicineAlternative } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { describePresentation } from '@/shared/format/presentation'
import { Notice } from '@/shared/ui/Notice/Notice'

import { fetchAlternatives } from '../api/fetchAlternatives'
import { AlternativeRow } from './AlternativeRow'

import styles from './AlternativesSection.module.css'

/** Quantas linhas aparecem antes do "mostrar mais". */
const VISIBLE = 6

/**
 * Abaixo disso a economia não ganha destaque. Nove centavos num comprimido de
 * sessenta e cinco não muda a decisão de ninguém.
 */
const RELEVANT_SAVINGS = 0.1

/** A embalagem escolhida na ficha, que é o ponto de comparação. */
export interface ComparisonReference {
  /** Nula quando a descrição não permite ler a dosagem. */
  milligrams: number | null
  /** "4 mg/ml": a dosagem das formas medidas, como gotas e xarope. */
  strength: string | null
  /**
   * Contada (comprimido, cápsula) ou medida (gotas, xarope, creme). Nula
   * quando não há embalagem escolhida.
   */
  form: 'counted' | 'measured' | null
  /** Liberação comum, prolongada ou retardada. Nula quando não se sabe. */
  release: 'prolonged' | 'delayed' | 'normal' | null
  /** Nulo em xarope, creme e embalagem sem quantidade legível. */
  perUnit: number | null
  unitName: string | null
}

interface AlternativesSectionProps {
  registrationNumber: string
  reference: ComparisonReference
}

/**
 * Medicamentos com o mesmo princípio ativo.
 *
 * Busca própria, separada da ficha: esta consulta é a mais cara das duas, e
 * juntá-las faria a ficha inteira esperar por ela.
 *
 * A comparação parte da embalagem escolhida na ficha, e não da que a API
 * escolhe para o registro: um mesmo registro pode ter 10 mg e 25 mg, e
 * comparar o comprimido de 25 mg com os de 10 mg dos outros seria errado.
 */
export function AlternativesSection({ registrationNumber, reference }: AlternativesSectionProps) {
  const { state } = useSelectedState()

  const result = useQuery<AlternativesResult>(
    (signal) => fetchAlternatives(registrationNumber, state, signal),
    [registrationNumber, state],
  )

  if (result.status === 'loading' || result.status === 'idle') {
    return <ApiLoading label="Buscando medicamentos equivalentes" shape="rows" />
  }

  // Falha aqui não derruba a ficha: o preço do medicamento consultado já está
  // na tela, e é a informação principal.
  if (result.status === 'error') {
    return <Notice tone="warning">Não foi possível carregar os equivalentes agora.</Notice>
  }

  const { alternatives, notice } = result.data
  const others = alternatives.filter((alternative) => !alternative.isCurrent)

  if (others.length === 0) {
    return (
      <section className={styles.section}>
        <h2 className={styles.title}>Mesmo princípio ativo</h2>
        <p className={styles.intro}>
          Nenhum outro medicamento com o mesmo princípio ativo tem preço publicado.
        </p>
      </section>
    )
  }

  // Mesma forma e mesma dosagem primeiro, que é a troca possível. As outras
  // ficam recolhidas: servem para comparar, mas não substituem sem
  // orientação. Gotas ao lado de comprimidos, com preço "por unidade" de um
  // e "a embalagem" do outro, não é comparação nenhuma.
  const sameDosage = others.filter((alternative) => isComparable(alternative, reference))
  const otherDosages = others.filter((alternative) => !sameDosage.includes(alternative))

  // Do mais barato ao mais caro pelo preço de uma unidade (comprimido, ml ou
  // grama), sem preço por unidade por último.
  const byUnitPrice = (a: MedicineAlternative, b: MedicineAlternative) =>
    (unitPrice(a) ?? Number.POSITIVE_INFINITY) - (unitPrice(b) ?? Number.POSITIVE_INFINITY)
  sameDosage.sort(byUnitPrice)

  const cheapest = sameDosage[0] ?? null
  const cheapestUnit = cheapest ? unitPrice(cheapest) : null

  const savings =
    reference.perUnit !== null && cheapestUnit !== null ? reference.perUnit - cheapestUnit : 0
  const savingsRate = reference.perUnit ? savings / reference.perUnit : 0

  return (
    <section className={styles.section} aria-labelledby="alternatives-title">
      <h2 className={styles.title} id="alternatives-title">
        Mesmo princípio ativo
      </h2>

      <p className={styles.intro}>{notice}</p>

      {cheapest && savingsRate >= RELEVANT_SAVINGS && (
        <p className={styles.savings}>
          <strong>{cheapest.name}</strong>
          {cheapest.manufacturer && `, da ${cheapest.manufacturer},`} sai{' '}
          <strong className="numeric">{formatPrice(savings)}</strong> mais barato por{' '}
          {reference.unitName}, {Math.round(savingsRate * 100)}% a menos, na mesma dosagem.
        </p>
      )}

      {sameDosage.length > 0 ? (
        <AlternativeList items={sameDosage} />
      ) : (
        <p className={styles.intro}>Nenhum equivalente na mesma forma e dosagem.</p>
      )}

      {otherDosages.length > 0 && (
        <details className={styles.more}>
          <summary>Outras dosagens e formas ({otherDosages.length})</summary>
          <AlternativeList items={otherDosages} />
        </details>
      )}
    </section>
  )
}

/**
 * Preço de uma unidade de um equivalente. Comprimido e cápsula a API já
 * calcula; nas formas medidas (gotas, xarope, creme) a conta é feita aqui,
 * pelo volume da descrição, porque a API só divide por unidades contadas.
 */
function unitPrice(alternative: MedicineAlternative): number | null {
  if (alternative.pricePerUnit !== null) {
    return alternative.pricePerUnit
  }

  const measure = describePresentation(alternative.presentation)?.measure
  return measure ? alternative.consumerPrice / measure.amount : null
}

function isComparable(alternative: MedicineAlternative, reference: ComparisonReference): boolean {
  if (reference.form === null) {
    return true
  }

  const readable = describePresentation(alternative.presentation)
  const counted = Boolean(readable?.units) || alternative.unitCount !== null

  // Liberação prolongada não troca por comum, mesmo com a mesma dosagem: a
  // dose diária muda. A retardada fica junto da comum de propósito: a fonte
  // escreve "LIB RETARD" em umas cápsulas de omeprazol e não em outras do
  // mesmo tipo, e separar esconderia equivalentes de verdade.
  if (
    reference.release !== null &&
    readable &&
    (readable.release === 'prolonged') !== (reference.release === 'prolonged')
  ) {
    return false
  }

  if (reference.form === 'counted') {
    return (
      counted &&
      (reference.milligrams === null || alternative.dosageInMilligrams === reference.milligrams)
    )
  }

  return (
    !counted &&
    readable !== null &&
    (reference.strength === null || readable.strength === reference.strength)
  )
}

function AlternativeList({ items }: { items: MedicineAlternative[] }) {
  const [expanded, setExpanded] = useState(false)

  const shown = expanded ? items : items.slice(0, VISIBLE)
  const hidden = items.length - shown.length

  return (
    <div className={styles.listBlock}>
      <ul className={styles.list}>
        {shown.map((alternative) => (
          <AlternativeRow key={alternative.registrationNumber} alternative={alternative} />
        ))}
      </ul>

      {hidden > 0 && (
        <button className={styles.showMore} type="button" onClick={() => setExpanded(true)}>
          Mostrar mais {hidden}
        </button>
      )}
    </div>
  )
}
