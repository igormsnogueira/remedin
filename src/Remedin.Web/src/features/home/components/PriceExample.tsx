import type { CSSProperties } from 'react'
import { Link } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { fetchAlternatives } from '@/features/medicine-alternatives/api/fetchAlternatives'
import { useQuery } from '@/shared/api/useQuery'
import type { AlternativesResult } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'

import styles from './PriceExample.module.css'

/**
 * Omeprazol 20 mg, cápsulas. Escolhido porque a diferença é grande, a dosagem
 * é a mesma nos dois lados e a contagem de unidades é legível — a comparação é
 * honesta (ADR 0011). Se o registro sair da lista da CMED, o bloco some.
 */
const EXAMPLE_REGISTRATION = '102980354'

/**
 * Um caso real de diferença de preço, calculado na hora.
 *
 * O número não fica escrito na página: o preço muda todo mês, e um valor fixo
 * ficaria falso sem ninguém perceber. Vem da mesma consulta de alternativas da
 * ficha do medicamento.
 *
 * Esta chamada também acorda a API enquanto a pessoa ainda está lendo ou
 * digitando, e a primeira busca dela chega sem a espera de servidor parado.
 */
export function PriceExample() {
  const { state } = useSelectedState()

  const example = useQuery<AlternativesResult>(
    (signal) => fetchAlternatives(EXAMPLE_REGISTRATION, state, signal),
    [state],
  )

  // Conteúdo de apoio: enquanto carrega ou se falhar, não ocupa espaço nem
  // mostra erro. A busca, logo acima, continua sendo o que importa.
  if (example.status !== 'success') {
    return null
  }

  const { alternatives, cheapestComparable, activeIngredient } = example.data
  const current = alternatives.find((alternative) => alternative.isCurrent)

  if (!current?.pricePerUnit || !cheapestComparable?.pricePerUnit) {
    return null
  }

  // "7 vezes" comunica melhor que "R$ 4,59 a menos" quando a diferença é
  // grande. Abaixo do dobro, a razão fica pequena demais para impressionar, e
  // o valor em reais diz mais.
  const highest = Math.max(current.pricePerUnit, cheapestComparable.pricePerUnit)

  const ratio = current.pricePerUnit / cheapestComparable.pricePerUnit
  const headline =
    ratio >= 2
      ? `${Math.round(ratio)} vezes`
      : formatPrice(current.pricePerUnit - cheapestComparable.pricePerUnit)
  const explanation =
    ratio >= 2
      ? 'a diferença de preço entre o mesmo princípio ativo, na mesma dosagem.'
      : 'a menos por unidade, com o mesmo princípio ativo e a mesma dosagem.'

  return (
    <section className={styles.example} aria-labelledby="example-title">
      <div className={styles.layout}>
        <div className={styles.prices}>
          <p className={styles.caption}>Exemplo real, preço máximo em {example.data.state}</p>
          <h2 className={styles.title} id="example-title">
            {capitalize(activeIngredient ?? current.name)} {current.dosageInMilligrams} mg, por unidade
          </h2>

          <dl className={styles.list}>
            <div className={styles.row}>
              <dt>
                {current.name}
                {current.manufacturer && <span className={styles.maker}>{current.manufacturer}</span>}
              </dt>
              <dd className="numeric">{formatPrice(current.pricePerUnit)}</dd>
            </div>
            <div className={styles.row}>
              <dt>
                {cheapestComparable.name}
                {cheapestComparable.manufacturer && (
                  <span className={styles.maker}>{cheapestComparable.manufacturer}</span>
                )}
              </dt>
              <dd className={`${styles.cheapest} numeric`}>
                {formatPrice(cheapestComparable.pricePerUnit)}
              </dd>
            </div>
          </dl>

          <Link className={styles.link} to={`/medicamento/${EXAMPLE_REGISTRATION}`}>
            Ver a comparação completa
          </Link>
        </div>

        <div className={styles.highlight}>
          <p className={styles.ratio}>{headline}</p>

          {/* A diferença em tamanho, para ser vista antes de ser lida. Os
              valores já estão escritos ao lado, então o leitor de tela pula. */}
          <div className={styles.bars} aria-hidden="true">
            <span className={styles.bar} style={barWidth(current.pricePerUnit, highest)} />
            <span
              className={`${styles.bar} ${styles.barCheapest}`}
              style={barWidth(cheapestComparable.pricePerUnit, highest)}
            />
          </div>

          <p className={styles.explanation}>{explanation}</p>
        </div>
      </div>
    </section>
  )
}

/** Largura da barra em relação ao maior preço, com um mínimo para não sumir. */
function barWidth(price: number, highest: number): CSSProperties {
  return { '--share': `${Math.max((price / highest) * 100, 3)}%` } as CSSProperties
}

/** "OMEPRAZOL" vira "Omeprazol". A fonte publica os nomes em maiúsculas. */
function capitalize(text: string): string {
  const lower = text.toLocaleLowerCase('pt-BR')
  return lower.charAt(0).toLocaleUpperCase('pt-BR') + lower.slice(1)
}
