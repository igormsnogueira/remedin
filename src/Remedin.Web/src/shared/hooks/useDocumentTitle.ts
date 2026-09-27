import { useEffect } from 'react'

const SUFFIX = 'Remedin'

/**
 * Título da aba.
 *
 * Numa aplicação de página única o título não muda sozinho, e quem abre três
 * medicamentos em três abas fica com três abas idênticas.
 */
export function useDocumentTitle(title: string | null) {
  useEffect(() => {
    document.title = title ? `${title} — ${SUFFIX}` : SUFFIX
  }, [title])
}
