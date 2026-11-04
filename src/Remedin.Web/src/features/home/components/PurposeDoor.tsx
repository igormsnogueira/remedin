import { Link } from 'react-router-dom'

import { fetchPurposes } from '@/features/medicine-purposes/api/fetchPurposes'
import { useQuery } from '@/shared/api/useQuery'
import type { PurposeSummary } from '@/shared/api/types'

import styles from './PurposeDoor.module.css'

/** Quantas finalidades a home mostra. As outras ficam a um clique. */
const SHOWN = 6

/**
 * A entrada pela finalidade, ao lado da busca pelo nome.
 *
 * Mostra as finalidades com mais medicamentos, com a contagem vinda da API.
 * Enquanto carrega, os espaços ficam reservados para a página não pular
 * quando a resposta chegar.
 */
export function PurposeDoor({ className }: { className?: string }) {
  const purposes = useQuery<PurposeSummary[]>(fetchPurposes, [])

  const shown =
    purposes.status === 'success'
      ? [...purposes.data].sort((a, b) => b.medicineCount - a.medicineCount).slice(0, SHOWN)
      : null

  return (
    <section className={className} aria-labelledby="door-purpose">
      <h2 className={styles.title} id="door-purpose">
        Buscar pela finalidade
      </h2>
      <p className={styles.text}>Escolha o que você precisa tratar.</p>

      {purposes.status !== 'error' && (
        <ul className={styles.grid} aria-busy={shown === null}>
          {shown === null
            ? Array.from({ length: SHOWN }, (_, index) => (
                <li className={styles.placeholder} key={index} aria-hidden="true" />
              ))
            : shown.map((purpose) => (
                <li key={purpose.code}>
                  <Link className={styles.tile} to={`/finalidades/${purpose.code}`}>
                    <span className={styles.label}>{purpose.label}</span>
                    <span className={styles.count}>
                      {purpose.medicineCount.toLocaleString('pt-BR')} medicamentos
                    </span>
                  </Link>
                </li>
              ))}
        </ul>
      )}

      <Link className={styles.all} to="/finalidades">
        Ver todas as finalidades
      </Link>
    </section>
  )
}
