import type { CSSProperties, ReactNode } from 'react'
import { Link } from 'react-router-dom'

import { formatPrice } from '@/shared/format/currency'
import { classNames } from '@/shared/ui/classNames'

import { EXAMPLE_REGISTRATION, withMaker } from '../model/useExample'
import type { Example, ExampleState } from '../model/useExample'

import styles from './LiveExample.module.css'

/**
 * Uma ficha de verdade no topo da home, no estado escolhido. O dado vem de
 * useExample, consultado uma vez para a página toda.
 */
export function LiveExample({ state, className }: { state: ExampleState; className?: string }) {
  // Apoio, não conteúdo principal: se falhar, some sem mostrar erro.
  if (state.status === 'error') {
    return null
  }

  if (state.status === 'loading') {
    return <Placeholder className={className} />
  }

  const { medicine: data, lowestPrice, comparison } = state.example

  return (
    <article className={classNames(styles.card, className)} aria-labelledby="example-name">
      <p className={styles.caption}>Exemplo real · preço máximo em {data.state}</p>

      <h2 className={styles.name} id="example-name">
        {data.name}
      </h2>
      {data.manufacturer && <p className={styles.maker}>{data.manufacturer}</p>}

      <dl className={styles.facts}>
        <Row order={0} label="Para que serve">
          {data.purpose ?? data.therapeuticClassName ?? 'Não informado'}
        </Row>

        <Row order={1} label="Receita">
          {data.requiresPrescription === null ? (
            'Não informado'
          ) : (
            <span
              className={classNames(
                styles.pill,
                data.requiresPrescription ? styles.pillRequired : styles.pillFree,
              )}
            >
              {data.requiresPrescription ? 'Exige receita' : 'Não precisa'}
            </span>
          )}
        </Row>

        {lowestPrice !== null && (
          <Row order={2} label="Preço máximo">
            <span className={styles.priceFrom}>a partir de </span>
            <span className={`${styles.price} numeric`}>{formatPrice(lowestPrice)}</span>
          </Row>
        )}
      </dl>

      {comparison && <Comparison comparison={comparison} />}

      <Link className={styles.link} to={`/medicamento/${EXAMPLE_REGISTRATION}`}>
        Ver a ficha completa
      </Link>
    </article>
  )
}

function Row({
  order,
  label,
  children,
}: {
  order: number
  label: string
  children: ReactNode
}) {
  return (
    <div className={styles.row} style={{ '--order': order } as CSSProperties}>
      <dt>{label}</dt>
      <dd>{children}</dd>
    </div>
  )
}

function Comparison({ comparison }: { comparison: NonNullable<Example['comparison']> }) {
  const { current, cheapest, ratio, unitName } = comparison

  return (
    <div className={styles.compare} style={{ '--order': 3 } as CSSProperties}>
      <p className={styles.compareTitle}>
        Em outra marca, a {unitName} sai <strong>{Math.round(ratio)} vezes</strong> mais barata.
      </p>

      <Bar label={withMaker(current)} value={current.pricePerUnit} share={100} muted />
      <Bar
        label={withMaker(cheapest)}
        value={cheapest.pricePerUnit}
        share={(cheapest.pricePerUnit / current.pricePerUnit) * 100}
      />
    </div>
  )
}

function Bar({
  label,
  value,
  share,
  muted = false,
}: {
  label: string
  value: number
  share: number
  muted?: boolean
}) {
  return (
    <div className={styles.bar}>
      <span className={styles.barLabel}>
        {label}
        <span className="numeric">{formatPrice(value)}</span>
      </span>
      <span
        className={classNames(styles.barFill, muted && styles.barMuted)}
        style={{ '--share': `${Math.max(share, 3)}%` } as CSSProperties}
        aria-hidden="true"
      />
    </div>
  )
}

/** O cartão no formato final, vazio, para a página não pular quando o dado chegar. */
function Placeholder({ className }: { className?: string }) {
  return (
    <div className={classNames(styles.card, styles.placeholder, className)} aria-hidden="true">
      <span className={styles.ghost} style={{ inlineSize: '45%' }} />
      <span className={styles.ghostTitle} />
      <span className={styles.ghost} style={{ inlineSize: '60%' }} />
      <span className={styles.ghostBlock} />
      <span className={styles.ghostBlock} />
      <span className={styles.ghostBlock} />
    </div>
  )
}
