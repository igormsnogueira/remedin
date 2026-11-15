import { useEffect, useState } from 'react'

export type QueryState<T> =
  | { status: 'idle' }
  | { status: 'loading' }
  | { status: 'success'; data: T; stale: boolean }
  | { status: 'error'; error: Error }

type Settled<T> =
  | { status: 'success'; data: T }
  | { status: 'error'; error: Error }

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
    // `request` muda a cada render; `key` representa as dependências declaradas.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key])

  if (!request) {
    return { status: 'idle' }
  }

  if (settled?.key !== key) {
    return settled?.result.status === 'success'
      ? { status: 'success', data: settled.result.data, stale: true }
      : { status: 'loading' }
  }

  return settled.result.status === 'success'
    ? { status: 'success', data: settled.result.data, stale: false }
    : settled.result
}
