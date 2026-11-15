import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'

import { fetchLocation } from '@/features/state-selection/api/fetchLocation'

import { SelectedStateContext } from './SelectedStateContext'
import type { StateOrigin } from './SelectedStateContext'

const MANUAL_KEY = 'remedin:state'

const DETECTED_KEY = 'remedin:state-detected'

const DEFAULT_STATE = 'SP'

interface Selection {
  state: string
  origin: StateOrigin
}

function read(key: string): string | null {
  try {
    return localStorage.getItem(key)
  } catch {
    return null
  }
}

function write(key: string, value: string): void {
  try {
    localStorage.setItem(key, value)
  } catch {
    // Armazenamento bloqueado: segue sem salvar.
  }
}

function initialSelection(): Selection {
  const manual = read(MANUAL_KEY)

  if (manual) {
    return { state: manual, origin: 'manual' }
  }

  const detected = read(DETECTED_KEY)

  return detected
    ? { state: detected, origin: 'detected' }
    : { state: DEFAULT_STATE, origin: 'default' }
}

export function SelectedStateProvider({ children }: { children: ReactNode }) {
  const [selection, setSelection] = useState(initialSelection)

  // Detecta o estado pela conexão; a escolha manual sempre vence.
  useEffect(() => {
    if (initialSelection().origin === 'manual') {
      return
    }

    const controller = new AbortController()

    fetchLocation(controller.signal).then(
      ({ state }) => {
        if (!state) {
          return
        }

        write(DETECTED_KEY, state)
        setSelection((current) =>
          current.origin === 'manual' ? current : { state, origin: 'detected' },
        )
      },
      () => {},
    )

    return () => controller.abort()
  }, [])

  const setState = useCallback((next: string) => {
    setSelection({ state: next, origin: 'manual' })
    write(MANUAL_KEY, next)
  }, [])

  const value = useMemo(
    () => ({ state: selection.state, origin: selection.origin, setState }),
    [selection, setState],
  )

  return <SelectedStateContext value={value}>{children}</SelectedStateContext>
}
