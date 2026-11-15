import { useEffect, useRef } from 'react'
import { useLocation } from 'react-router-dom'

export function useFocusOnNavigation<T extends HTMLElement>() {
  const { pathname } = useLocation()
  const target = useRef<T>(null)
  // Compara com a rota anterior, e não com "primeira renderização": no StrictMode o efeito roda duas vezes.
  const previous = useRef(pathname)

  useEffect(() => {
    if (previous.current === pathname) {
      return
    }
    previous.current = pathname

    window.scrollTo({ top: 0 })
    target.current?.focus({ preventScroll: true })
  }, [pathname])

  return target
}
