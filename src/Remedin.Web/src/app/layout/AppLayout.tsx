import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'

import { useFavorites } from '@/features/favorites/model/useFavorites'
import { HeaderSearch } from '@/features/medicine-search/components/HeaderSearch'
import { WakeUpNotice } from '@/shared/api/WakeUpNotice'
import { classNames } from '@/shared/ui/classNames'
import { Logo } from '@/shared/ui/Logo/Logo'

import styles from './AppLayout.module.css'
import { useFocusOnNavigation } from './useFocusOnNavigation'

export function AppLayout() {
  const main = useFocusOnNavigation<HTMLElement>()

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
          <p>
            Estado sugerido pela conexão com dados de{' '}
            <a href="https://db-ip.com" rel="noopener">
              IP Geolocation by DB-IP
            </a>
            .
          </p>
        </div>
      </footer>
    </div>
  )
}

function navClass({ isActive }: { isActive: boolean }) {
  return classNames(styles.navLink, isActive && styles.navLinkActive)
}
