import { useCallback } from 'react'
import { useSearchParams } from 'react-router-dom'

/**
 * A página atual, guardada na barra de endereço.
 *
 * Na URL e não em estado do componente: assim o botão voltar do navegador
 * desfaz a troca de página, e o link que a pessoa copia abre onde ela estava.
 *
 * Trocar de página rola de volta ao topo. Sem isso, quem clica em "Próxima"
 * continua no fim da tela, olhando o rodapé de uma lista nova.
 */
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

      // A regra de movimento reduzido do CSS não alcança rolagem disparada por
      // código, então a preferência é consultada aqui.
      const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches

      window.scrollTo({ top: 0, behavior: reducedMotion ? 'auto' : 'smooth' })
    },
    [setParams],
  )

  return [page, goTo]
}
