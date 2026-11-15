import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'

import { StateSelect } from '@/features/state-selection/components/StateSelect'
import { Button } from '@/shared/ui/Button/Button'
import { TextField } from '@/shared/ui/Field/TextField'

import styles from './SearchForm.module.css'

interface SearchFormProps {
  initialTerm: string
  onSearch: (term: string) => void
  hideLabel?: boolean
}

export function SearchForm({ initialTerm, onSearch, hideLabel = false }: SearchFormProps) {
  const [term, setTerm] = useState(initialTerm)
  const input = useRef<HTMLInputElement>(null)

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
      <form className={styles.form} onSubmit={handleSubmit} role="search">
        <TextField
          className={styles.term}
          label="Remédio, substância ou sintoma"
          hideLabel={hideLabel}
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
