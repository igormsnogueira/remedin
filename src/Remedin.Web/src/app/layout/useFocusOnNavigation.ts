import { useEffect, useRef } from 'react'
import { useLocation } from 'react-router-dom'

/**
 * Move o foco para o conteúdo a cada troca de rota.
 *
 * Numa aplicação de página única o navegador não recarrega, então o foco fica
 * onde estava: no link que a pessoa clicou, que já não existe mais. Quem usa
 * teclado precisa percorrer o cabeçalho de novo a cada navegação, e o leitor de
 * tela não anuncia que a tela mudou.
 *
 * A primeira renderização fica de fora — o foco no carregamento inicial é do
 * navegador, e roubá-lo atrapalha quem chegou pelo endereço direto.
 */
export function useFocusOnNavigation<T extends HTMLElement>() {
  const { pathname } = useLocation()
  const target = useRef<T>(null)
  const firstRender = useRef(true)

  useEffect(() => {
    if (firstRender.current) {
      firstRender.current = false
      return
    }

    target.current?.focus()
  }, [pathname])

  return target
}
