const baseUrl = import.meta.env.VITE_API_URL

if (!baseUrl) {
  // Falhar aqui, no carregamento, é melhor que uma tela em branco com erro de
  // rede na primeira busca.
  throw new Error('VITE_API_URL não configurada. Copie .env.example para .env.')
}

/**
 * Erro de uma chamada que chegou ao servidor e voltou com status de falha.
 * Separado do erro de rede porque a interface trata os dois de forma
 * diferente: um pede "tente de novo", o outro é problema do que foi pedido.
 */
export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }

  get isNotFound(): boolean {
    return this.status === 404
  }
}

interface ErrorBody {
  erro?: string
}

/**
 * Uma chamada GET à API.
 *
 * O tipo de retorno é uma afirmação, não uma verificação: `T` descreve o que a
 * API promete, e ninguém confere em tempo de execução. É aceitável porque as
 * duas pontas estão neste repositório e a API é a única origem do dado. Com
 * fonte externa, entraria validação de esquema aqui.
 */
export async function get<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  return (await response.json()) as T
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as ErrorBody
    return body.erro ?? `A requisição falhou com status ${response.status}.`
  } catch {
    return `A requisição falhou com status ${response.status}.`
  }
}

/** Monta a query string ignorando o que estiver vazio. */
export function query(params: Record<string, string | number | undefined>): string {
  const entries = Object.entries(params).filter(
    ([, value]) => value !== undefined && value !== '',
  )

  if (entries.length === 0) {
    return ''
  }

  const search = new URLSearchParams(entries.map(([key, value]) => [key, String(value)]))

  return `?${search.toString()}`
}
