import { useEffect, useState } from 'react'

/**
 * Estados possíveis de uma chamada, num tipo só.
 *
 * União discriminada em vez de três campos soltos: com `data`, `loading` e
 * `error` independentes, existem combinações impossíveis (carregando e com
 * erro ao mesmo tempo) que o componente precisa lembrar de tratar. Aqui o
 * compilador só libera `data` depois que `status` foi verificado.
 */
export type QueryState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T }
  | { status: 'error'; error: Error }

type Settled<T> =
  | { status: 'success'; data: T }
  | { status: 'error'; error: Error }

/**
 * Busca dado da API quando as dependências mudam, e cancela a chamada anterior.
 *
 * O cancelamento não é detalhe: trocar de estado durante uma busca dispara duas
 * requisições, e sem ele a resposta da antiga pode chegar depois e sobrescrever
 * o resultado certo pelo velho.
 *
 * Só a resposta é guardada em estado. "Carregando" é deduzido de o resultado
 * guardado ser de uma consulta anterior, e não gravado — gravar exigiria um
 * setState dentro do efeito, que provoca uma renderização a mais a cada busca.
 *
 * @param request recebe o sinal de cancelamento e devolve a promessa da chamada.
 *   Nulo quando ainda não há o que buscar, como termo de busca vazio.
 * @param dependencies valores que definem a consulta. Precisam ser
 *   serializáveis, porque é a identidade deles que diz se a resposta guardada
 *   ainda vale.
 */
export function useQuery<T>(
  request: ((signal: AbortSignal) => Promise<T>) | null,
  dependencies: readonly unknown[],
): QueryState<T> {
  const key = JSON.stringify(dependencies)
  const [settled, setSettled] = useState<{ key: string; result: Settled<T> } | null>(null)

  useEffect(() => {
    if (!request) {
      return
    }

    const controller = new AbortController()

    request(controller.signal)
      .then((data) => setSettled({ key, result: { status: 'success', data } }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return
        }

        setSettled({
          key,
          result: {
            status: 'error',
            error: error instanceof Error ? error : new Error('Falha inesperada.'),
          },
        })
      })

    return () => controller.abort()
    // A identidade de `request` muda a cada render, então quem chama declara
    // de que valores a busca depende, e `key` os representa.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key])

  if (!request) {
    return { status: 'idle' }
  }

  if (settled?.key !== key) {
    return { status: 'loading' }
  }

  return settled.result
}
