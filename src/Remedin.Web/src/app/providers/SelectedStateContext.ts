import { createContext } from 'react'

export interface SelectedState {
  state: string
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
