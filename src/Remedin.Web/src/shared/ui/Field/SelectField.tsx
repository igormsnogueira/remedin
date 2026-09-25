import { useId } from 'react'
import type { SelectHTMLAttributes } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Field.module.css'

interface SelectFieldProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'id'> {
  label: string
  hideLabel?: boolean
}

export function SelectField({
  label,
  hideLabel = false,
  className,
  children,
  ...rest
}: SelectFieldProps) {
  const id = useId()

  return (
    <div className={classNames(styles.field, className)}>
      <label className={classNames(styles.label, hideLabel && 'visually-hidden')} htmlFor={id}>
        {label}
      </label>
      <select className={styles.control} id={id} {...rest}>
        {children}
      </select>
    </div>
  )
}
