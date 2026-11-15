import type { ReactNode } from 'react'

import type { ExampleState } from '../model/useExample'
import { LiveExample } from './LiveExample'
import { PurposeChips } from './PurposeChips'

import styles from './HomeHero.module.css'

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
