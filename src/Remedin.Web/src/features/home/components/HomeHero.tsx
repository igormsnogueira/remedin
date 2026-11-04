import type { ReactNode } from 'react'

import { Link } from 'react-router-dom'

import { HeroIllustration } from './HeroIllustration'
import { PurposeDoor } from './PurposeDoor'

import styles from './HomeHero.module.css'

/**
 * Buscas de exemplo, que também mostram o tipo de coisa que se pode digitar:
 * um princípio ativo, um nome comercial e um sintoma.
 */
const EXAMPLES = ['dipirona', 'losartana', 'colesterol']

/**
 * Topo da home: o que o site responde, e as duas formas de começar.
 *
 * Busca e finalidade aparecem lado a lado e com o mesmo peso. Para que serve o
 * remédio é uma das perguntas que o Remedin responde, e não um caminho
 * alternativo para quem esqueceu o nome.
 *
 * Recebe a busca pronta em vez de montá-la: o formulário pertence à feature de
 * busca, e a home só decide onde ele aparece.
 */
export function HomeHero({ search }: { search: ReactNode }) {
  return (
    <section className={styles.hero} aria-labelledby="home-title">
      <div className={styles.heading}>
        <div className={styles.text}>
          <p className={styles.eyebrow}>Consulta pública de medicamentos</p>

          <h1 className={styles.title} id="home-title">
            Para que serve o seu remédio e quanto a farmácia pode cobrar por ele
          </h1>

          <p className={styles.intro}>
            Veja para que serve cada medicamento, se precisa de receita e o preço máximo no seu
            estado. Cada farmácia define o próprio preço, mas nenhuma pode passar desse teto.
          </p>
        </div>

        <HeroIllustration className={styles.illustration} />
      </div>

      <div className={styles.doors}>
        <section className={styles.door} aria-labelledby="door-name">
          <h2 className={styles.doorTitle} id="door-name">
            Buscar pelo nome
          </h2>
          <p className={styles.doorText}>Do remédio, da substância ou do que você sente.</p>
          {search}

          <p className={styles.examples}>
            Experimente:{' '}
            {EXAMPLES.map((example, index) => (
              <span key={example}>
                {index > 0 && ' · '}
                <Link to={`/?q=${encodeURIComponent(example)}`}>{example}</Link>
              </span>
            ))}
          </p>
        </section>

        <PurposeDoor className={styles.door} />
      </div>
    </section>
  )
}
