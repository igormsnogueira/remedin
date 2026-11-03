import { Link, NavLink, Outlet } from 'react-router-dom'

import { classNames } from '@/shared/ui/classNames'
import { Logo } from '@/shared/ui/Logo/Logo'

import styles from './AppLayout.module.css'
import { useFocusOnNavigation } from './useFocusOnNavigation'

/**
 * A moldura de todas as telas: cabeçalho, conteúdo e rodapé.
 *
 * As páginas ficam no <Outlet>, então nenhuma delas precisa saber da largura
 * máxima nem das margens laterais.
 */
export function AppLayout() {
  const main = useFocusOnNavigation<HTMLElement>()

  return (
    <div className={styles.shell}>
      <a className={styles.skip} href="#conteudo">
        Pular para o conteúdo
      </a>

      <header className={styles.header}>
        <div className={classNames(styles.container, styles.bar)}>
          <Link className={styles.brand} to="/" aria-label="Remedin, página inicial">
            <Logo />
          </Link>

          <nav aria-label="Principal">
            <ul className={styles.nav}>
              <li>
                {/* end: sem ele, "/" ficaria ativo em todas as páginas. */}
                <NavLink className={navClass} to="/" end>
                  Buscar
                </NavLink>
              </li>
              <li>
                <NavLink className={navClass} to="/finalidades">
                  Por finalidade
                </NavLink>
              </li>
            </ul>
          </nav>
        </div>
      </header>

      <main
        className={classNames(styles.main, styles.container)}
        id="conteudo"
        ref={main}
        // -1 aceita foco por código sem entrar na ordem do Tab, que é o que
        // permite mover o foco para cá na troca de rota.
        tabIndex={-1}
      >
        <Outlet />
      </main>

      <footer className={styles.footer}>
        <div className={classNames(styles.container, styles.footerContent)}>
          <p>
            Preços da Câmara de Regulação do Mercado de Medicamentos (CMED) e registros da ANVISA,
            atualizados toda semana.
          </p>
          <p>Consulta informativa. Não substitui a orientação do médico ou do farmacêutico.</p>
        </div>
      </footer>
    </div>
  )
}

function navClass({ isActive }: { isActive: boolean }) {
  return classNames(styles.navLink, isActive && styles.navLinkActive)
}
