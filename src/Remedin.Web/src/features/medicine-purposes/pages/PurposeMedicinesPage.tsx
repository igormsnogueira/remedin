import { Link, useParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
// O cartão vem da busca em vez de shared/ui: ele conhece o formato de
// MedicineSummary e a rota do medicamento, então não é primitivo de interface.
// Se uma terceira tela precisar dele, aí sim vira componente compartilhado.
import { ResultCard } from '@/features/medicine-search/components/ResultCard'
import { StateSelect } from '@/features/state-selection/components/StateSelect'
import { ApiLoading } from '@/shared/api/ApiLoading'
import { ApiError } from '@/shared/api/client'
import { useQuery } from '@/shared/api/useQuery'
import type { PurposeMedicines } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { usePageParam } from '@/shared/hooks/usePageParam'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Pagination } from '@/shared/ui/Pagination/Pagination'

import { fetchPurposeMedicines } from '../api/fetchPurposes'
import { PurposeIcon } from '../components/PurposeIcon'
import { purposeStyle } from '../model/purposeTheme'

import styles from './PurposeMedicinesPage.module.css'

export function PurposeMedicinesPage() {
  const { code } = useParams()
  const [page, goToPage] = usePageParam()
  const { state } = useSelectedState()

  const purpose = useQuery<PurposeMedicines>(
    code ? (signal) => fetchPurposeMedicines(code, state, page, signal) : null,
    [code, state, page],
  )

  useDocumentTitle(purpose.status === 'success' ? purpose.data.label : null)

  if (purpose.status === 'loading' || purpose.status === 'idle') {
    return <ApiLoading label="Carregando os medicamentos" />
  }

  if (purpose.status === 'error') {
    const notFound = purpose.error instanceof ApiError && purpose.error.isNotFound

    return (
      <div className={styles.page}>
        <Notice tone="danger">
          {notFound ? 'Finalidade não encontrada.' : purpose.error.message}
        </Notice>
        <Link to="/finalidades">Ver todas as finalidades</Link>
      </div>
    )
  }

  const { label, medicines, state: uf } = purpose.data

  return (
    <div className={styles.page}>
      {/* Na cor da finalidade, e com o mesmo nome de transição do bloco da
          lista: o bloco clicado se expande até virar este topo. */}
      <header
        className={`purpose-theme ${styles.banner}`}
        style={{ ...purposeStyle(code), viewTransitionName: `purpose-${code}` }}
      >
        <PurposeIcon code={code} size="lg" onTint />

        <div className={styles.bannerText}>
          <Link className={styles.back} to="/finalidades" viewTransition>
            Finalidades
          </Link>
          <h1 className={styles.bannerTitle}>{label}</h1>
          <p className={`${styles.bannerMeta} numeric`}>
            {medicines.total.toLocaleString('pt-BR')} medicamentos com preço em {uf}
          </p>
        </div>

        <StateSelect className={styles.state} />
      </header>

      {medicines.items.length === 0 ? (
        <Notice>
          {page > 1
            ? 'Não há mais medicamentos desta finalidade além desta página.'
            : `Nenhum medicamento desta finalidade tem preço publicado em ${uf}.`}
        </Notice>
      ) : (
        <section className={styles.list}>
          <p className={styles.count}>Do mais barato ao mais caro, pelo preço máximo em {uf}</p>

          {medicines.items.map((medicine) => (
            <ResultCard key={medicine.registrationNumber} medicine={medicine} />
          ))}

          <Pagination page={medicines} onChange={goToPage} busy={purpose.stale} />
        </section>
      )}

      <p>
        <Link to="/finalidades">Ver todas as finalidades</Link>
      </p>
    </div>
  )
}
