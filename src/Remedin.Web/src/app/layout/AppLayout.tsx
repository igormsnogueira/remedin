import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'

import { useFavorites } from '@/features/favorites/model/useFavorites'
import { HeaderSearch } from '@/features/medicine-search/components/HeaderSearch'
import { WakeUpNotice } from '@/shared/api/WakeUpNotice'
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

  // A home e os resultados já têm a busca grande no topo da página. Repetir no
  // cabeçalho seria dois campos para a mesma coisa na mesma tela.
  const { pathname } = useLocation()
  const showHeaderSearch = pathname !== '/'

  const { favorites } = useFavorites()

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

          {showHeaderSearch && <HeaderSearch className={styles.search} />}

          {/* Sem link para o início: o logo já faz isso, que é o que todo
              mundo espera de um logo. */}
          <nav className={styles.navArea} aria-label="Principal">
            <ul className={styles.nav}>
              <li>
                <NavLink className={navClass} to="/finalidades">
                  Finalidades
                </NavLink>
              </li>
              <li>
                <NavLink className={navClass} to="/favoritos">
                  Favoritos
                  {favorites.length > 0 && (
                    <span className={styles.badge}>
                      {favorites.length}
                      <span className="visually-hidden"> salvos</span>
                    </span>
                  )}
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

      <WakeUpNotice />

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
