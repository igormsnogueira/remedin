import type { ButtonHTMLAttributes } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Button.module.css'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary'
}

export function Button({ variant = 'secondary', className, ...rest }: ButtonProps) {
  return (
    <button
      className={classNames(styles.button, variant === 'primary' && styles.primary, className)}
      {...rest}
    />
  )
}
