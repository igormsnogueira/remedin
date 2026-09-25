import type { ReactNode } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Notice.module.css'

interface NoticeProps {
  tone?: 'info' | 'warning' | 'danger'
  children: ReactNode
}

/** Recado curto ao lado do conteúdo: ausência de dado, alerta, erro de busca. */
export function Notice({ tone = 'info', children }: NoticeProps) {
  return (
    <p
      className={classNames(styles.notice, styles[tone])}
      // Aviso de erro precisa ser anunciado por leitor de tela quando aparece,
      // porque quem não vê a tela não percebe que algo mudou.
      role={tone === 'danger' ? 'alert' : undefined}
    >
      {children}
    </p>
  )
}
