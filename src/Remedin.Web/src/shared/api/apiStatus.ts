/**
 * Se a API está acordando, para o site inteiro.
 *
 * O servidor gratuito desliga depois de um tempo sem acesso e leva de 30 a 60
 * segundos para voltar. Cada tela tem o próprio carregando, mas a explicação
 * do porquê da espera é uma só, e fica num aviso global (WakeUpNotice).
 *
 * Fica fora do React, como os favoritos: o cliente HTTP registra aqui, e o
 * aviso lê com useSyncExternalStore.
 */

/**
 * Com a API acordada, as respostas chegam em menos de meio segundo. Passar
 * disso quase sempre significa servidor desligado.
 */
const SLOW_AFTER_MS = 2500

export type ApiPhase =
  /** Nenhuma resposta ainda, e nada demorando. */
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

/** Quando a espera começou, para o aviso contar os segundos. */
export function getWakingSince(): number | null {
  return wakingSince
}

export function subscribeApiPhase(listener: () => void): () => void {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

/**
 * Chamado pelo cliente HTTP a cada requisição. Devolve a função de término,
 * para ser chamada quando a resposta chegar, com sucesso ou não.
 */
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

    // Erro de rede não prova que o servidor acordou. Resposta com erro HTTP
    // prova: alguém do outro lado respondeu.
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

/** O aviso chama depois de mostrar o "pronto" por um instante. */
export function settleApiPhase(): void {
  if (phase === 'woke') {
    setPhase('awake')
  }
}

function setPhase(next: ApiPhase): void {
  phase = next
  listeners.forEach((listener) => listener())
}
