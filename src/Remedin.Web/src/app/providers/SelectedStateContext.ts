import { createContext } from 'react'

/**
 * De onde veio o estado escolhido. Decide se a escolha pode ser trocada
 * sozinha pela detecção.
 *
 * - manual: a pessoa escolheu. Nada muda sem ela.
 * - detected: estimado pela conexão. Uma detecção nova pode trocá-lo.
 * - default: nenhuma das duas ainda; vale SP.
 */
export type StateOrigin = 'manual' | 'detected' | 'default'

export interface SelectedState {
  state: string
  origin: StateOrigin
  /** Escolha da pessoa: vale para sempre neste navegador. */
  setState: (state: string) => void
}

/**
 * O estado escolhido, disponível em qualquer tela.
 *
 * Sobe até aqui porque o preço-teto depende do ICMS estadual: toda chamada de
 * busca e de ficha leva a sigla, e trocar de estado na ficha e voltar para a
 * busca não pode perder a escolha.
 *
 * Em arquivo separado do provider para que o módulo do provider exporte só
 * componente — é a regra que mantém o hot reload do Vite funcionando.
 */
export const SelectedStateContext = createContext<SelectedState | null>(null)
