import { Link, useSearchParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { useQuery } from '@/shared/api/useQuery'
import type { SearchResults } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { usePageParam } from '@/shared/hooks/usePageParam'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Pagination } from '@/shared/ui/Pagination/Pagination'
import { Spinner } from '@/shared/ui/Spinner/Spinner'

import { searchMedicines } from '../api/searchMedicines'
import { ResultCard } from '../components/ResultCard'
import { SearchForm } from '../components/SearchForm'

import styles from './SearchPage.module.css'

export function SearchPage() {
  // O termo vive na URL, não em useState: assim a busca é compartilhável e o
  // botão voltar do navegador funciona.
  const [params, setParams] = useSearchParams()
  const term = params.get('q') ?? ''

  const [page, goToPage] = usePageParam()
  const { state } = useSelectedState()

  useDocumentTitle(term ? `${term} — busca` : null)

  const results = useQuery<SearchResults>(
    term ? (signal) => searchMedicines(term, state, page, signal) : null,
    [term, state, page],
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

      {/* Buscar de novo volta para a primeira página: continuar na página 3 de
          um resultado que não existe mais mostraria uma tela vazia. */}
      <SearchForm
        initialTerm={term}
        onSearch={(value) => setParams(value ? { q: value } : {})}
      />

      <p className={styles.browse}>
        Não sabe o nome? <Link to="/finalidades">Veja por finalidade</Link>.
      </p>

      {results.status === 'loading' && <Spinner label="Buscando medicamentos" />}

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
            {/* polite anuncia a contagem quando a lista muda, sem interromper
                quem está digitando. */}
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
