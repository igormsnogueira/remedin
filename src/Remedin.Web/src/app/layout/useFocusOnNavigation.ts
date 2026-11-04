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
 * O carregamento inicial fica de fora — o foco ali é do navegador, e roubá-lo
 * atrapalha quem chegou pelo endereço direto. A comparação é com a rota
 * anterior, e não um "primeira renderização": o StrictMode roda o efeito duas
 * vezes em desenvolvimento, e a segunda vez roubava o foco e rolava a página.
 */
export function useFocusOnNavigation<T extends HTMLElement>() {
  const { pathname } = useLocation()
  const target = useRef<T>(null)
  const previous = useRef(pathname)

  useEffect(() => {
    if (previous.current === pathname) {
      return
    }
    previous.current = pathname

    // Sem preventScroll, o foco rolaria até o <main> e esconderia o cabeçalho.
    // A página nova começa do topo, como num site comum.
    window.scrollTo({ top: 0 })
    target.current?.focus({ preventScroll: true })
  }, [pathname])

  return target
}
