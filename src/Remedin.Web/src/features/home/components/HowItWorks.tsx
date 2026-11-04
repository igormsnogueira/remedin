import styles from './HowItWorks.module.css'

/**
 * Como usar, em três passos numerados.
 *
 * Lista ordenada e não três cartões com ícone: a ordem é a informação, e o
 * número já a comunica.
 */
export function HowItWorks() {
  return (
    <section aria-labelledby="how-title">
      <h2 className={styles.title} id="how-title">
        Como consultar
      </h2>

      <ol className={styles.steps}>
        <li className={styles.step}>
          <span className={styles.number} aria-hidden="true">
            1
          </span>
          <div>
            <p className={styles.stepTitle}>Busque ou escolha a finalidade</p>
            <p className={styles.stepText}>
              Pelo nome que está na caixa, pela substância ou pelo que você precisa tratar.
            </p>
          </div>
        </li>
        <li className={styles.step}>
          <span className={styles.number} aria-hidden="true">
            2
          </span>
          <div>
            <p className={styles.stepTitle}>Escolha o seu estado</p>
            <p className={styles.stepText}>
              O preço máximo muda conforme o ICMS de cada estado.
            </p>
          </div>
        </li>
        <li className={styles.step}>
          <span className={styles.number} aria-hidden="true">
            3
          </span>
          <div>
            <p className={styles.stepTitle}>Veja a ficha e compare</p>
            <p className={styles.stepText}>
              Para que serve, se precisa de receita e o preço máximo. As opções com o mesmo
              princípio ativo aparecem por preço de comprimido.
            </p>
          </div>
        </li>
      </ol>
    </section>
  )
}
