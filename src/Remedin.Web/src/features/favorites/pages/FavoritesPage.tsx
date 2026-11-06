import { useRef } from 'react'
import { Link } from 'react-router-dom'

import { StateSelect } from '@/features/state-selection/components/StateSelect'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'

import { FavoriteCard } from '../components/FavoriteCard'
import { useFavorites } from '../model/useFavorites'

import styles from './FavoritesPage.module.css'

export function FavoritesPage() {
  const { favorites, remove } = useFavorites()
  const summary = useRef<HTMLParagraphElement>(null)

  useDocumentTitle('Favoritos')

  // O botão clicado some junto com o cartão. Sem mover o foco, ele cairia no
  // começo da página, e quem usa teclado ou leitor de tela perderia o lugar.
  function handleRemove(registrationNumber: string) {
    remove(registrationNumber)
    summary.current?.focus()
  }

  return (
    <div className={styles.page}>
      <div className={styles.heading}>
        <h1>Favoritos</h1>
        <p className={styles.intro}>
          Os remédios que você salvou, com o preço máximo de hoje. A lista fica só neste navegador:
          o Remedin não guarda nada sobre você.
        </p>
      </div>

      {favorites.length === 0 ? (
        <div className={styles.empty}>
          <Notice>
            Nenhum remédio salvo ainda. Na ficha do medicamento ou nos resultados da busca, use o
            botão Favorito para guardar aqui.
          </Notice>
          <Link to="/">Buscar um remédio</Link>
        </div>
      ) : (
        <>
          <div className={styles.toolbar}>
            <p className={styles.count} ref={summary} tabIndex={-1} aria-live="polite">
              {favorites.length === 1 ? '1 remédio salvo' : `${favorites.length} remédios salvos`}
            </p>
            <StateSelect className={styles.state} />
          </div>

          <ul className={styles.list}>
            {favorites.map((favorite) => (
              <li key={favorite.registrationNumber}>
                <FavoriteCard favorite={favorite} onRemove={handleRemove} />
              </li>
            ))}
          </ul>
        </>
      )}
    </div>
  )
}
