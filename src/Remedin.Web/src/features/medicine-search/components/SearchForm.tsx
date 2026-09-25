import { useState } from 'react'
import type { FormEvent } from 'react'

import { StateSelect } from '@/features/state-selection/components/StateSelect'
import { Button } from '@/shared/ui/Button/Button'
import { TextField } from '@/shared/ui/Field/TextField'

import styles from './SearchForm.module.css'

interface SearchFormProps {
  initialTerm: string
  onSearch: (term: string) => void
}

export function SearchForm({ initialTerm, onSearch }: SearchFormProps) {
  const [term, setTerm] = useState(initialTerm)

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    onSearch(term.trim())
  }

  return (
    // <form> em vez de botão com onClick: assim Enter no campo busca, que é
    // como a maioria das pessoas usa.
    <form className={styles.form} onSubmit={handleSubmit} role="search">
      <TextField
        className={styles.term}
        label="Nome do medicamento ou princípio ativo"
        placeholder="dipirona, omeprazol, losartana..."
        value={term}
        onChange={(event) => setTerm(event.target.value)}
        autoFocus
      />

      <StateSelect />

      <Button type="submit" variant="primary">
        Buscar
      </Button>
    </form>
  )
}
