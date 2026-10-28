import { useEffect, useState } from 'react'

import { Spinner } from '@/shared/ui/Spinner/Spinner'

import styles from './ApiLoading.module.css'

/**
 * Tempo a partir do qual a espera deixa de parecer normal.
 *
 * Com a API acordada, as respostas chegam em menos de meio segundo. Passar
 * disso quase sempre significa que o servidor estava desligado.
 */
const SLOW_AFTER_MS = 4000

/**
 * Indicador de carregamento de dado da API, com explicação quando demora.
 *
 * O servidor da API desliga depois de um tempo sem acesso e leva cerca de 25
 * segundos para voltar. Sem aviso, esse tempo olhando um ícone girando parece
 * site quebrado, e a pessoa vai embora antes de a resposta chegar.
 *
 * Fica aqui, junto do cliente HTTP, e não em shared/ui: o motivo da espera é
 * um fato sobre a API, e o Spinner continua sendo um primitivo sem opinião.
 */
export function ApiLoading({ label }: { label: string }) {
  const slow = useElapsed(SLOW_AFTER_MS)

  return (
    <div className={styles.loading}>
      <Spinner label={label} />

      {/* polite: quem usa leitor de tela também precisa saber por que está
          demorando, sem que o aviso interrompa o que estiver sendo lido. */}
      <p className={styles.hint} aria-live="polite">
        {slow &&
          'O servidor estava em repouso e está ligando. Na primeira consulta isso pode levar até meio minuto.'}
      </p>
    </div>
  )
}

function useElapsed(milliseconds: number): boolean {
  const [elapsed, setElapsed] = useState(false)

  useEffect(() => {
    const timer = window.setTimeout(() => setElapsed(true), milliseconds)
    return () => window.clearTimeout(timer)
  }, [milliseconds])

  return elapsed
}
