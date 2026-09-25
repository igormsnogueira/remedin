import { useCallback, useMemo, useState } from 'react'
import type { ReactNode } from 'react'

import { SelectedStateContext } from './SelectedStateContext'

const STORAGE_KEY = 'remedin:state'
const DEFAULT_STATE = 'SP'

function readStoredState(): string {
  try {
    return localStorage.getItem(STORAGE_KEY) ?? DEFAULT_STATE
  } catch {
    // Navegação anônima com armazenamento bloqueado. A escolha vale só para
    // esta sessão, e o site continua funcionando.
    return DEFAULT_STATE
  }
}

export function SelectedStateProvider({ children }: { children: ReactNode }) {
  const [state, setSelected] = useState(readStoredState)

  const setState = useCallback((next: string) => {
    setSelected(next)

    try {
      localStorage.setItem(STORAGE_KEY, next)
    } catch {
      // Sem persistir, mas sem quebrar a troca de estado.
    }
  }, [])

  const value = useMemo(() => ({ state, setState }), [state, setState])

  return <SelectedStateContext value={value}>{children}</SelectedStateContext>
}
