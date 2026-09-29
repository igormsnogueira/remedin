import { useSelectedState } from '@/app/providers/useSelectedState'
import { useQuery } from '@/shared/api/useQuery'
import type { AlternativesResult } from '@/shared/api/types'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Spinner } from '@/shared/ui/Spinner/Spinner'

import { fetchAlternatives } from '../api/fetchAlternatives'
import { AlternativeRow } from './AlternativeRow'
import { SavingsHighlight } from './SavingsHighlight'

import styles from './AlternativesSection.module.css'

/**
 * Medicamentos com o mesmo princípio ativo.
 *
 * Busca própria, separada da ficha: esta consulta é a mais cara das duas, e
 * juntá-las faria a ficha inteira esperar por ela.
 */
export function AlternativesSection({ registrationNumber }: { registrationNumber: string }) {
  const { state } = useSelectedState()

  const result = useQuery<AlternativesResult>(
    (signal) => fetchAlternatives(registrationNumber, state, signal),
    [registrationNumber, state],
  )

  if (result.status === 'loading' || result.status === 'idle') {
    return <Spinner label="Buscando medicamentos equivalentes" />
  }

  // Falha aqui não derruba a ficha: o preço do medicamento consultado já está
  // na tela, e é a informação principal.
  if (result.status === 'error') {
    return <Notice tone="warning">Não foi possível carregar os equivalentes agora.</Notice>
  }

  const { alternatives, cheapestComparable, savingsPerUnit, notice } = result.data
  const others = alternatives.filter((alternative) => !alternative.isCurrent)

  if (others.length === 0) {
    return (
      <section>
        <h2>Outras opções</h2>
        <Notice>
          Não encontramos outro medicamento com o mesmo princípio ativo e preço publicado.
        </Notice>
      </section>
    )
  }

  const referenceDosage =
    alternatives.find((alternative) => alternative.isCurrent)?.dosageInMilligrams ?? null

  return (
    <section className={styles.section}>
      <h2>Outras opções com o mesmo princípio ativo</h2>

      {savingsPerUnit !== null && cheapestComparable && (
        <SavingsHighlight savingsPerUnit={savingsPerUnit} cheapest={cheapestComparable} />
      )}

      <Notice tone="warning">{notice}</Notice>

      <ul className={styles.list}>
        {alternatives.map((alternative) => (
          <AlternativeRow
            key={alternative.registrationNumber}
            alternative={alternative}
            referenceDosage={referenceDosage}
          />
        ))}
      </ul>
    </section>
  )
}
