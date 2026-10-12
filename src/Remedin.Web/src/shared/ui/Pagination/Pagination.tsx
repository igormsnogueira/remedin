import type { PagedResults } from '@/shared/api/types'
import { Button } from '@/shared/ui/Button/Button'

import styles from './Pagination.module.css'

interface PaginationProps {
  page: PagedResults<unknown>
  onChange: (pageNumber: number) => void
  /** Desabilita os botões enquanto a página pedida ainda não chegou. */
  busy?: boolean
}

export function Pagination({ page, onChange, busy = false }: PaginationProps) {
  if (!page.hasPrevious && !page.hasNext) {
    return null
  }

  return (
    <nav className={styles.pagination} aria-label="Páginas de resultados">
      <p className={styles.range}>
        Mostrando {page.firstPosition} a {page.lastPosition} de{' '}
        {page.total.toLocaleString('pt-BR')}
      </p>

      <div className={styles.controls}>
        <Button
          type="button"
          disabled={!page.hasPrevious || busy}
          onClick={() => onChange(page.pageNumber - 1)}
        >
          Anterior
        </Button>
        <Button
          type="button"
          disabled={!page.hasNext || busy}
          onClick={() => onChange(page.pageNumber + 1)}
        >
          Próxima
        </Button>
      </div>
    </nav>
  )
}
