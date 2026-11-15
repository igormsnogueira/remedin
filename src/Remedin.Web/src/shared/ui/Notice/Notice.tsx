import type { ReactNode } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Notice.module.css'

interface NoticeProps {
  tone?: 'info' | 'warning' | 'danger'
  children: ReactNode
}

export function Notice({ tone = 'info', children }: NoticeProps) {
  return (
    <p
      className={classNames(styles.notice, styles[tone])}
      role={tone === 'danger' ? 'alert' : undefined}
    >
      {children}
    </p>
  )
}
