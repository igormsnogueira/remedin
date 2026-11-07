import type { ReactNode } from 'react'

import type { ExampleState } from '../model/useExample'
import { LiveExample } from './LiveExample'
import { PurposeChips } from './PurposeChips'

import styles from './HomeHero.module.css'

/**
 * Topo da home: uma faixa verde de ponta a ponta com a pergunta, a busca e as
 * finalidades, e ao lado uma ficha de verdade, consultada na hora.
 *
 * A ficha ao lado mostra o produto funcionando, em vez de uma ilustração: é o
 * mesmo dado que a pessoa vai ver depois de buscar.
 *
 * Recebe a busca pronta em vez de montá-la: o formulário pertence à feature de
 * busca, e a home só decide onde ele aparece.
 */
export function HomeHero({ search, example }: { search: ReactNode; example: ExampleState }) {
  return (
    <section className={styles.band} aria-labelledby="home-title" data-full-bleed>
      <div className={styles.grid}>
        <div className={styles.text}>
          <p className={styles.eyebrow}>Consulta pública de medicamentos</p>

          <h1 className={styles.title} id="home-title">
            Para que serve o seu remédio e quanto a farmácia pode cobrar
          </h1>

          <p className={styles.intro}>
            Cada farmácia define o próprio preço, mas nenhuma pode passar do teto da CMED. Veja o
            teto no seu estado, para que serve o remédio e se precisa de receita.
          </p>

          <div className={styles.search}>{search}</div>

          <PurposeChips />
        </div>

        <LiveExample className={styles.example} state={example} />
      </div>

      <p className={styles.sources}>
        Preços da CMED e registros da ANVISA · os 27 estados · atualizado toda semana
      </p>
    </section>
  )
}
