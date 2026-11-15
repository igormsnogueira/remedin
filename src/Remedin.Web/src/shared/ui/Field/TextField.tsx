import { useId } from 'react'
import type { InputHTMLAttributes, Ref } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Field.module.css'

interface TextFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id'> {
  label: string
  hideLabel?: boolean
  ref?: Ref<HTMLInputElement>
}

export function TextField({ label, hideLabel = false, className, ...rest }: TextFieldProps) {
  const id = useId()

  return (
    <div className={classNames(styles.field, className)}>
      <label className={classNames(styles.label, hideLabel && 'visually-hidden')} htmlFor={id}>
        {label}
      </label>
      <input className={styles.control} id={id} {...rest} />
    </div>
  )
}
