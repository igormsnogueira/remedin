import { Link, Outlet } from 'react-router-dom'

import styles from './AppLayout.module.css'

/**
 * A moldura de todas as telas: cabeçalho, conteúdo e rodapé.
 *
 * As páginas ficam no <Outlet>, então nenhuma delas precisa saber da largura
 * máxima nem das margens laterais.
 */
export function AppLayout() {
  return (
    <div className={styles.shell}>
      <a className={styles.skip} href="#conteudo">
        Pular para o conteúdo
      </a>

      <header className={styles.header}>
        <div className={styles.container}>
          <Link className={styles.brand} to="/">
            Remedin
          </Link>
        </div>
      </header>

      <main className={`${styles.main} ${styles.container}`} id="conteudo">
        <Outlet />
      </main>

      <footer className={styles.footer}>
        <div className={styles.container}>
          Preços da Câmara de Regulação do Mercado de Medicamentos (CMED) e dados de registro da
          ANVISA. Esta é uma consulta informativa e não substitui orientação de médico ou
          farmacêutico.
        </div>
      </footer>
    </div>
  )
}
