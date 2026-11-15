import { Link, useSearchParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { HomePage } from '@/features/home/pages/HomePage'
import { ApiLoading } from '@/shared/api/ApiLoading'
import { useQuery } from '@/shared/api/useQuery'
import type { SearchResults } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { usePageParam } from '@/shared/hooks/usePageParam'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Pagination } from '@/shared/ui/Pagination/Pagination'

import { searchMedicines } from '../api/searchMedicines'
import { ResultCard } from '../components/ResultCard'
import { SearchForm } from '../components/SearchForm'

import styles from './SearchPage.module.css'

export function SearchPage() {
  const [params, setParams] = useSearchParams()
  const term = params.get('q') ?? ''

  const [page, goToPage] = usePageParam()
  const { state } = useSelectedState()

  useDocumentTitle(term ? `${term} — busca` : null)

  const results = useQuery<SearchResults>(
    term ? (signal) => searchMedicines(term, state, page, signal) : null,
    [term, state, page],
  )

  const form = (
    <SearchForm
      key={term}
      initialTerm={term}
      onSearch={(value) => setParams(value ? { q: value } : {})}
      hideLabel={!term}
    />
  )

  if (!term) {
    return <HomePage search={form} />
  }

  return (
    <div className={styles.page}>
      <h1 className={styles.resultsTitle}>Resultados para “{term}”</h1>

      {form}

      <p className={styles.browse}>
        Não sabe o nome? <Link to="/finalidades">Veja por finalidade</Link>.
      </p>

      {results.status === 'loading' && <ApiLoading label="Buscando medicamentos" />}

      {results.status === 'error' && <Notice tone="danger">{results.error.message}</Notice>}

      {results.status === 'success' &&
        (results.data.medicines.items.length === 0 ? (
          <Notice>
            {page > 1
              ? `Não há mais resultados para "${term}" além desta página.`
              : `Nenhum medicamento encontrado para "${term}". Confira a grafia, ou tente pelo princípio ativo.`}
          </Notice>
        ) : (
          <section className={styles.results}>
            <p className={styles.count} aria-live="polite">
              {results.data.medicines.total.toLocaleString('pt-BR')} resultados · preço para{' '}
              {results.data.state}
            </p>

            {results.data.medicines.items.map((medicine) => (
              <ResultCard key={medicine.registrationNumber} medicine={medicine} />
            ))}

            <Pagination
              page={results.data.medicines}
              onChange={goToPage}
              busy={results.stale}
            />
          </section>
        ))}
    </div>
  )
}
