import { trackRequest } from './apiStatus'

const baseUrl = import.meta.env.VITE_API_URL

if (!baseUrl) {
  throw new Error('VITE_API_URL não configurada. Copie .env.example para .env.')
}

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

export async function get<T>(path: string, signal?: AbortSignal): Promise<T> {
  const done = trackRequest()
  let response: Response

  try {
    response = await fetch(`${baseUrl}${path}`, {
      headers: { Accept: 'application/json' },
      signal,
    })
  } catch (error) {
    done(false)
    throw error
  }

  done(true)

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

export function wakeUpApi(): void {
  const done = trackRequest()

  fetch(`${baseUrl}/health`, { cache: 'no-store' }).then(
    () => done(true),
    () => done(false),
  )
}

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
