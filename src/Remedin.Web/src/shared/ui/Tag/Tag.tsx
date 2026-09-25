import type { ReactNode } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Tag.module.css'

interface TagProps {
  tone?: 'accent' | 'muted'
  children: ReactNode
}

export function Tag({ tone = 'accent', children }: TagProps) {
  return <span className={classNames(styles.tag, tone === 'muted' && styles.muted)}>{children}</span>
}
