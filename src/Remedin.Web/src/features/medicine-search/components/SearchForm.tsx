import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'

import { StateSelect } from '@/features/state-selection/components/StateSelect'
import { Button } from '@/shared/ui/Button/Button'
import { TextField } from '@/shared/ui/Field/TextField'

import styles from './SearchForm.module.css'

interface SearchFormProps {
  initialTerm: string
  onSearch: (term: string) => void
  /**
   * Esconde o rótulo do campo na tela, mantendo-o para o leitor de tela. Para
   * quando um título ao redor já diz o que digitar.
   */
  hideLabel?: boolean
}

export function SearchForm({ initialTerm, onSearch, hideLabel = false }: SearchFormProps) {
  const [term, setTerm] = useState(initialTerm)
  const input = useRef<HTMLInputElement>(null)

  // Foco no campo ao abrir, sem o autoFocus do React: ele rola a página até o
  // campo, e na home isso cortava o cabeçalho em telas mais baixas. Só com
  // mouse: na tela de toque o teclado abriria cobrindo metade da página.
  useEffect(() => {
    if (window.matchMedia('(pointer: fine)').matches) {
      input.current?.focus({ preventScroll: true })
    }
  }, [])

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    onSearch(term.trim())
  }

  return (
    <div className={styles.container}>
      {/* <form> em vez de botão com onClick: assim Enter no campo busca, que é
          como a maioria das pessoas usa. */}
      <form className={styles.form} onSubmit={handleSubmit} role="search">
        <TextField
          className={styles.term}
          label="Remédio, substância ou sintoma"
          hideLabel={hideLabel}
          // Um sintoma no exemplo, porque a busca também encontra pela
          // finalidade em linguagem comum.
          placeholder="dipirona, losartana, colesterol"
          value={term}
          onChange={(event) => setTerm(event.target.value)}
          ref={input}
        />

        <StateSelect />

        <Button type="submit" variant="primary">
          Consultar
        </Button>
      </form>
    </div>
  )
}
