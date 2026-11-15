import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'

export function usePageParam(): [number, (pageNumber: number) => void] {
  const [params, setParams] = useSearchParams()

  const current = Number(params.get('pagina') ?? 1)
  const page = Number.isInteger(current) && current > 0 ? current : 1

  const goTo = useCallback(
    (pageNumber: number) => {
      setParams((previous) => {
        const next = new URLSearchParams(previous)

        if (pageNumber <= 1) {
          next.delete('pagina')
        } else {
          next.set('pagina', String(pageNumber))
        }

        return next
      })

      const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches

      window.scrollTo({ top: 0, behavior: reducedMotion ? 'auto' : 'smooth' })
    },
    [setParams],
  )

  return [page, goTo]
}
