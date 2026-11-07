import { useEffect, useState, useSyncExternalStore } from 'react'

import { LogoMark } from '@/shared/ui/Logo/Logo'

import { getApiPhase, getWakingSince, settleApiPhase, subscribeApiPhase } from './apiStatus'

import styles from './WakeUpNotice.module.css'

/** Quanto tempo o "pronto" fica na tela antes de o aviso sair. */
const READY_FOR_MS = 1600

/**
 * Aviso de servidor ligando, para o site todo.
 *
 * Aparece só quando uma chamada demora além do normal, e uma vez por visita.
 * Explica o motivo, mostra que algo está andando (a barra e os segundos) e
 * some sozinho quando a API responde. Não bloqueia a página: a pessoa pode
 * continuar lendo ou digitando a busca enquanto espera.
 */
export function WakeUpNotice() {
  const phase = useSyncExternalStore(subscribeApiPhase, getApiPhase, () => 'idle' as const)
  const seconds = useSecondsSince(phase === 'waking' ? getWakingSince() : null)

  useEffect(() => {
    if (phase !== 'woke') {
      return
    }

    const timer = window.setTimeout(settleApiPhase, READY_FOR_MS)
    return () => window.clearTimeout(timer)
  }, [phase])

  const visible = phase === 'waking' || phase === 'woke'

  return (
    // role="status" sempre presente, e o conteúdo muda dentro dele: é assim
    // que o leitor de tela anuncia a mudança sem interromper a leitura.
    <div className={styles.region} role="status">
      {visible && (
        <div className={styles.notice} data-ready={phase === 'woke'}>
          <span className={styles.mark} aria-hidden="true">
            <LogoMark size={36} />
          </span>

          <div className={styles.body}>
            <p className={styles.title}>
              {phase === 'woke' ? 'Pronto!' : 'Preparando os preços para você'}
            </p>

            {phase === 'waking' && (
              // Sem "servidor" nem "API": quem usa o site não precisa saber o
              // porquê técnico, só que está andando e que é só desta vez.
              <p className={styles.text}>
                A primeira consulta pode levar até um minuto. Depois disso, tudo fica rápido.
              </p>
            )}

            <div className={styles.progress} aria-hidden="true">
              <span className={styles.bar} />
            </div>
          </div>

          {phase === 'waking' && (
            <span className={`${styles.seconds} numeric`} aria-hidden="true">
              {seconds}s
            </span>
          )}
        </div>
      )}
    </div>
  )
}

function useSecondsSince(since: number | null): number {
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    if (since === null) {
      return
    }

    const timer = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(timer)
  }, [since])

  return since === null ? 0 : Math.max(0, Math.round((now - since) / 1000))
}
