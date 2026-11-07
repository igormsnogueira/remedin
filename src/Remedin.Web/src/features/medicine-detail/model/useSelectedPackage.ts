import { useSearchParams } from 'react-router-dom'

import type { PackageOption } from './packages'

const PARAM = 'embalagem'

/**
 * A embalagem que a pessoa escolheu, guardada na URL.
 *
 * Na URL e não em estado do componente: o link copiado abre na mesma
 * embalagem, e o botão voltar desfaz a troca. Sem escolha, ou com um código
 * que não existe mais, vale a mais barata.
 */
export function useSelectedPackage(options: PackageOption[]) {
  const [params, setParams] = useSearchParams()
  const key = params.get(PARAM)

  const selected = options.find((option) => option.key === key) ?? options[0] ?? null

  function select(next: string) {
    setParams(
      (current) => {
        current.set(PARAM, next)
        return current
      },
      // Trocar de embalagem não é navegar: sem replace, o voltar do navegador
      // passaria por cada embalagem clicada antes de sair da página.
      { replace: true, preventScrollReset: true },
    )
  }

  return { selected, select }
}
