import { createContext } from 'react'

export type StateOrigin = 'manual' | 'detected' | 'default'

export interface SelectedState {
  state: string
  origin: StateOrigin
  setState: (state: string) => void
}

export const SelectedStateContext = createContext<SelectedState | null>(null)
