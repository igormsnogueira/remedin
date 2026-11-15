import { useEffect } from 'react'

const SUFFIX = 'Remedin'

export function useDocumentTitle(title: string | null) {
  useEffect(() => {
    document.title = title ? `${title} — ${SUFFIX}` : SUFFIX
  }, [title])
}
