import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'

import { classNames } from '@/shared/ui/classNames'

import styles from './HeaderSearch.module.css'

/**
 * Busca compacta do cabeçalho, para as páginas internas.
 *
 * Quem está na ficha de um remédio e quer consultar outro não deveria precisar
 * voltar para o início. Usa o estado já escolhido, por isso não tem seletor: a
 * troca de estado acontece na própria página de resultados.
 */
export function HeaderSearch({ className }: { className?: string }) {
  const [term, setTerm] = useState('')
  const navigate = useNavigate()

  function handleSubmit(event: FormEvent) {
    event.preventDefault()

    const value = term.trim()

    if (value) {
      navigate(`/?q=${encodeURIComponent(value)}`)
      setTerm('')
    }
  }

  return (
    <form className={classNames(styles.form, className)} onSubmit={handleSubmit} role="search">
      <label className="visually-hidden" htmlFor="header-search">
        Buscar remédio, substância ou sintoma
      </label>
      <input
        className={styles.input}
        id="header-search"
        type="search"
        placeholder="Buscar outro remédio"
        value={term}
        onChange={(event) => setTerm(event.target.value)}
      />
    </form>
  )
}
