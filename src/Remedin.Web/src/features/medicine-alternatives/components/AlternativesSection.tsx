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

const VISIBLE = 6

const RELEVANT_SAVINGS = 0.1

export interface ComparisonReference {
  milligrams: number | null
  strength: string | null
  form: 'counted' | 'measured' | null
  release: 'prolonged' | 'delayed' | 'normal' | null
  perUnit: number | null
  unitName: string | null
}

interface AlternativesSectionProps {
  registrationNumber: string
  reference: ComparisonReference
}

export function AlternativesSection({ registrationNumber, reference }: AlternativesSectionProps) {
  const { state } = useSelectedState()

  const result = useQuery<AlternativesResult>(
    (signal) => fetchAlternatives(registrationNumber, state, signal),
    [registrationNumber, state],
  )

  if (result.status === 'loading' || result.status === 'idle') {
    return <ApiLoading label="Buscando medicamentos equivalentes" shape="rows" />
  }

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

  // Compara pela embalagem escolhida: o mesmo registro pode ter 10 mg e 25 mg.
  const sameDosage = others.filter((alternative) => isComparable(alternative, reference))
  const otherDosages = others.filter((alternative) => !sameDosage.includes(alternative))

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

  // Liberação prolongada não substitui a comum. A retardada fica junto porque a fonte nem sempre a marca.
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
