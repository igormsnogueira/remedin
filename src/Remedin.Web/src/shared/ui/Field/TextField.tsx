import { useId } from 'react'
import type { InputHTMLAttributes } from 'react'

import { classNames } from '@/shared/ui/classNames'

import styles from './Field.module.css'

interface TextFieldProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'id'> {
  label: string
  /** Esconde o rótulo na tela sem tirá-lo do leitor de tela. */
  hideLabel?: boolean
}

export function TextField({ label, hideLabel = false, className, ...rest }: TextFieldProps) {
  // useId garante que o for do label case com o id do input mesmo com dois
  // campos iguais na mesma página.
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
