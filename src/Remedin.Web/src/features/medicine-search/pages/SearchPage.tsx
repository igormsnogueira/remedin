import { useSearchParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { useQuery } from '@/shared/api/useQuery'
import type { SearchResults } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Spinner } from '@/shared/ui/Spinner/Spinner'

import { SEARCH_LIMIT, searchMedicines } from '../api/searchMedicines'
import { ResultCard } from '../components/ResultCard'
import { SearchForm } from '../components/SearchForm'

import styles from './SearchPage.module.css'

export function SearchPage() {
  // O termo vive na URL, não em useState: assim a busca é compartilhável e o
  // botão voltar do navegador funciona.
  const [params, setParams] = useSearchParams()
  const term = params.get('q') ?? ''

  const { state } = useSelectedState()

  useDocumentTitle(term ? `${term} — busca` : null)

  const results = useQuery<SearchResults>(
    term ? (signal) => searchMedicines(term, state, signal) : null,
    [term, state],
  )

  return (
    <div className={styles.page}>
      <div>
        <h1>Consulte o preço máximo de um medicamento</h1>
        <p className={styles.intro}>
          O preço que a farmácia pode cobrar tem teto definido por lei, e ele muda conforme o
          estado. Busque pelo nome do produto ou pelo princípio ativo.
        </p>
      </div>

      <SearchForm
        initialTerm={term}
        onSearch={(value) => setParams(value ? { q: value } : {})}
      />

      {results.status === 'loading' && <Spinner label="Buscando medicamentos" />}

      {results.status === 'error' && <Notice tone="danger">{results.error.message}</Notice>}

      {results.status === 'success' &&
        (results.data.medicines.length === 0 ? (
          <Notice>
            Nenhum medicamento encontrado para "{term}". Confira a grafia, ou tente pelo princípio
            ativo.
          </Notice>
        ) : (
          <section className={styles.results}>
            {/* polite anuncia a contagem quando a lista muda, sem interromper
                quem está digitando. */}
            <p className={styles.count} aria-live="polite">
              {results.data.medicines.length} resultados · preço para {results.data.state}
            </p>

            {results.data.medicines.map((medicine) => (
              <ResultCard key={medicine.registrationNumber} medicine={medicine} />
            ))}

            {results.data.medicines.length === SEARCH_LIMIT && (
              <Notice>
                A lista mostra os {SEARCH_LIMIT} mais relevantes. Se o seu medicamento não está
                aqui, busque pelo nome do produto em vez do princípio ativo.
              </Notice>
            )}
          </section>
        ))}
    </div>
  )
}
