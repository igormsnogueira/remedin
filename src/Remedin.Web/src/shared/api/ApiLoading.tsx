import type { ReactNode } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './ApiLoading.module.css'

type Shape = 'list' | 'detail' | 'grid' | 'rows'

interface ApiLoadingProps {
  /** O que está sendo carregado, para quem usa leitor de tela. */
  label: string
  /** O formato do que vai aparecer, para o esqueleto ter a mesma cara. */
  shape?: Shape
}

/**
 * Carregando de dado da API, no formato do conteúdo que vem.
 *
 * Esqueleto em vez de ícone girando: a página já aparece com a forma final, e
 * quando o dado chega ele ocupa o mesmo espaço, sem pular. O motivo de uma
 * espera longa (servidor ligando) fica no aviso global, WakeUpNotice, e não
 * repetido em cada pedaço da tela.
 */
export function ApiLoading({ label, shape = 'list' }: ApiLoadingProps) {
  return (
    <div className={classNames(styles.loading, styles[shape])} role="status">
      <span className="visually-hidden">{label}</span>
      {SHAPES[shape]}
    </div>
  )
}

const card = (key: number) => (
  <div className={styles.card} key={key} aria-hidden="true">
    <span className={styles.line} style={{ inlineSize: '45%' }} />
    <span className={styles.lineSm} style={{ inlineSize: '30%' }} />
    <span className={styles.lineSm} style={{ inlineSize: '60%' }} />
  </div>
)

const SHAPES: Record<Shape, ReactNode> = {
  list: [0, 1, 2, 3].map(card),
  rows: [0, 1, 2].map(card),
  grid: [0, 1, 2, 3, 4, 5, 6, 7].map((key) => (
    <div className={styles.tile} key={key} aria-hidden="true" />
  )),
  detail: (
    <>
      <div className={styles.banner} aria-hidden="true">
        <span className={styles.circle} />
        <span className={styles.title} />
      </div>
      <div className={styles.panel} aria-hidden="true">
        {[0, 1, 2].map((key) => (
          <span className={styles.line} key={key} style={{ inlineSize: `${70 - key * 15}%` }} />
        ))}
      </div>
      {[0, 1].map(card)}
    </>
  ),
}
