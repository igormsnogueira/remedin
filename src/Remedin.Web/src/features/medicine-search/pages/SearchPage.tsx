import { Link, useSearchParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { HomeHero } from '@/features/home/components/HomeHero'
import { HowItWorks } from '@/features/home/components/HowItWorks'
import { PriceExample } from '@/features/home/components/PriceExample'
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

  // A key recria o formulário quando o termo da URL muda. Sem ela, o campo
  // guardaria o texto antigo depois de um atalho da home ou do botão voltar.
  //
  // Buscar de novo volta para a primeira página: continuar na página 3 de um
  // resultado que não existe mais mostraria uma tela vazia.
  const form = (
    <SearchForm
      key={term}
      initialTerm={term}
      onSearch={(value) => setParams(value ? { q: value } : {})}
      // Na home o cartão em volta já tem título e explicação do que digitar.
      hideLabel={!term}
    />
  )

  // Sem termo, a mesma rota é a apresentação do site. Assim os links de busca
  // compartilhados continuam abrindo direto nos resultados.
  if (!term) {
    return (
      <div className={styles.home}>
        <HomeHero search={form} />
        <PriceExample />
        <HowItWorks />
      </div>
    )
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
