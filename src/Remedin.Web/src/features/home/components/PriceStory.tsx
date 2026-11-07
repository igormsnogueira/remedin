import type { CSSProperties } from 'react'

import { formatPrice } from '@/shared/format/currency'

import { withMaker } from '../model/useExample'
import type { ExampleState } from '../model/useExample'

import styles from './PriceStory.module.css'

/**
 * Como usar, contado pela rolagem.
 *
 * Em tela grande e navegador com animação ligada à rolagem, a seção fica
 * presa na tela e os três passos se trocam conforme a pessoa desce, cada um
 * com a sua cena ao lado. Nos outros casos (celular, navegador sem suporte,
 * pedido de menos movimento) é uma lista comum, um passo embaixo do outro.
 * A decisão é toda do CSS; o conteúdo é o mesmo.
 *
 * As cenas usam o mesmo exemplo real do topo. Sem ele, mostram o passo sem
 * números.
 */
export function PriceStory({ example }: { example: ExampleState }) {
  const ready = example.status === 'ready' ? example.example : null

  return (
    <section className={styles.story} aria-labelledby="story-title">
      <div className={styles.stage}>
        <h2 className={styles.title} id="story-title">
          Do nome do remédio ao preço certo
        </h2>

        <ol className={styles.steps}>
          <li className={styles.step} style={{ '--step': 0 } as CSSProperties}>
            <StepText
              number={1}
              title="Busque como você conhece o remédio"
              text="Pelo nome da caixa, pela substância ou pelo que você sente. A busca entende acento faltando e erro de digitação."
            />
            <div className={styles.scene}>
              <SearchScene
                term={ready?.medicine.activeIngredient ?? ready?.medicine.name ?? 'omeprazol'}
                results={
                  ready
                    ? [ready.medicine.name, ...ready.others.map((other) => other.name)]
                        .slice(0, 4)
                        .map((name, index) => ({
                          name,
                          maker:
                            index === 0
                              ? ready.medicine.manufacturer
                              : (ready.others[index - 1]?.manufacturer ?? null),
                        }))
                    : []
                }
              />
            </div>
          </li>

          <li className={styles.step} style={{ '--step': 1 } as CSSProperties}>
            <StepText
              number={2}
              title="Veja o teto no seu estado"
              text="O preço máximo muda com o ICMS de cada estado. A farmácia pode cobrar menos, nunca mais."
            />
            <div className={styles.scene}>
              <div className={styles.card}>
                <p className={styles.sceneLabel}>Preço máximo em {ready?.medicine.state ?? 'SP'}</p>
                <p className={`${styles.bigPrice} numeric`}>
                  {ready?.lowestPrice != null ? formatPrice(ready.lowestPrice) : 'R$ —'}
                </p>
                <p className={styles.sceneNote}>
                  {ready?.medicine.name ?? 'Remédio'} · teto da CMED com ICMS de{' '}
                  {ready?.medicine.icmsRate ?? '—'}%
                </p>
              </div>
            </div>
          </li>

          <li className={styles.step} style={{ '--step': 2 } as CSSProperties}>
            <StepText
              number={3}
              title="Compare com as outras marcas"
              text="Mesmo princípio ativo, mesma dosagem, preço por comprimido. A diferença costuma surpreender."
            />
            <div className={styles.scene}>
              {ready?.comparison ? (
                <div className={styles.card}>
                  <p className={styles.ratio}>
                    {Math.round(ready.comparison.ratio)}
                    <span className={styles.times}>×</span>
                  </p>
                  <p className={styles.sceneNote}>
                    a diferença por {ready.comparison.unitName} entre{' '}
                    {withMaker(ready.comparison.current)} e {withMaker(ready.comparison.cheapest)}:{' '}
                    <span className="numeric">
                      {formatPrice(ready.comparison.current.pricePerUnit)}
                    </span>{' '}
                    contra{' '}
                    <span className="numeric">
                      {formatPrice(ready.comparison.cheapest.pricePerUnit)}
                    </span>
                    .
                  </p>
                </div>
              ) : (
                <div className={styles.card}>
                  <p className={styles.sceneNote}>
                    Os equivalentes aparecem na ficha, do mais barato ao mais caro por unidade.
                  </p>
                </div>
              )}
            </div>
          </li>
        </ol>

        {/* Quanto da história já passou. Só aparece na versão presa à tela. */}
        <div className={styles.progress} aria-hidden="true" />
      </div>
    </section>
  )
}

function StepText({ number, title, text }: { number: number; title: string; text: string }) {
  return (
    <div className={styles.text}>
      <span className={styles.number} aria-hidden="true">
        {String(number).padStart(2, '0')}
      </span>
      <h3 className={styles.stepTitle}>{title}</h3>
      <p className={styles.stepText}>{text}</p>
    </div>
  )
}

function SearchScene({
  term,
  results,
}: {
  term: string
  results: { name: string; maker: string | null }[]
}) {
  // Desenho de uma busca, não uma busca: fica fora do leitor de tela, e o
  // texto do passo já diz o que ela mostra.
  return (
    <div className={styles.card} aria-hidden="true">
      <div className={styles.fakeInput}>
        <span className={styles.typed}>{term.toLowerCase()}</span>
        <span className={styles.caret} />
      </div>

      <ul className={styles.fakeResults}>
        {(results.length > 0 ? results : [null, null, null]).map((result, index) => (
          <li key={index} className={styles.fakeResult}>
            {result ? (
              <>
                <span className={styles.fakeName}>{result.name}</span>
                {result.maker && <span className={styles.fakeMaker}>{result.maker}</span>}
              </>
            ) : (
              <span className={styles.fakeBar} />
            )}
          </li>
        ))}
      </ul>
    </div>
  )
}
