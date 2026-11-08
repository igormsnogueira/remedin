import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'

import { fetchLocation } from '@/features/state-selection/api/fetchLocation'

import { SelectedStateContext } from './SelectedStateContext'
import type { StateOrigin } from './SelectedStateContext'

/** A escolha da pessoa. A chave é a mesma de antes: quem já escolheu continua escolhido. */
const MANUAL_KEY = 'remedin:state'

/**
 * O último estado detectado. Guardado para a próxima visita já abrir nele,
 * sem esperar a API, enquanto a detecção roda de novo por trás.
 */
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
    // Navegação anônima com armazenamento bloqueado. A escolha vale só para
    // esta sessão, e o site continua funcionando.
    return null
  }
}

function write(key: string, value: string): void {
  try {
    localStorage.setItem(key, value)
  } catch {
    // Sem persistir, mas sem quebrar a troca de estado.
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

  // Detecta pela conexão uma vez por visita, se a pessoa ainda não escolheu.
  // O resultado só vale se ela não tiver escolhido enquanto a resposta vinha:
  // a escolha manual sempre ganha.
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
      // Sem palpite, fica o que já estava (o último detectado ou SP). Não é
      // erro que a pessoa precise ver.
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
