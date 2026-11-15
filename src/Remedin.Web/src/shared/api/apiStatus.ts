const SLOW_AFTER_MS = 2500

export type ApiPhase =
  | 'idle'
  /** Uma chamada passou do tempo normal: o servidor está ligando. */
  | 'waking'
  /** Respondeu depois de ter demorado. O aviso mostra "pronto" e some. */
  | 'woke'
  /** Já respondeu nesta visita. Daqui em diante, nada de aviso. */
  | 'awake'

let phase: ApiPhase = 'idle'
let wakingSince: number | null = null
const listeners = new Set<() => void>()

export function getApiPhase(): ApiPhase {
  return phase
}

export function getWakingSince(): number | null {
  return wakingSince
}

export function subscribeApiPhase(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

export function trackRequest(): (reachedServer: boolean) => void {
  const startedAt = Date.now()

  const timer =
    phase === 'idle'
      ? window.setTimeout(() => {
          if (phase === 'idle') {
            wakingSince = startedAt
            setPhase('waking')
          }
        }, SLOW_AFTER_MS)
      : undefined

  return (reachedServer) => {
    window.clearTimeout(timer)

    if (!reachedServer) {
      return
    }

    if (phase === 'waking') {
      setPhase('woke')
    } else if (phase === 'idle') {
      setPhase('awake')
    }
  }
}

export function settleApiPhase(): void {
  if (phase === 'woke') {
    setPhase('awake')
  }
}

function setPhase(next: ApiPhase): void {
  phase = next
  listeners.forEach((listener) => listener())
}
