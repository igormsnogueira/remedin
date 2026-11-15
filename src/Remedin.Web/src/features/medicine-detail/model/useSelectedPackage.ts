import { useSearchParams } from 'react-router-dom'

import type { PackageOption } from './packages'

const PARAM = 'embalagem'

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
      // Trocar de embalagem não é navegar: sem replace, o voltar passaria por cada clique.
      { replace: true, preventScrollReset: true },
    )
  }

  return { selected, select }
}
