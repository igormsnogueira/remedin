import { useContext } from 'react'

import { SelectedStateContext } from './SelectedStateContext'
import type { SelectedState } from './SelectedStateContext'

export function useSelectedState(): SelectedState {
  const context = useContext(SelectedStateContext)

  if (!context) {
    throw new Error('useSelectedState precisa estar dentro de SelectedStateProvider.')
  }

  return context
}
