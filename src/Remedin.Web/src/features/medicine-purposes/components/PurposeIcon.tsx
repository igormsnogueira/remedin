import { classNames } from '@/shared/ui/classNames'

import { purposeStyle, purposeTheme } from '../model/purposeTheme'

import styles from './PurposeIcon.module.css'

interface PurposeIconProps {
  code: string | null | undefined
  size?: 'sm' | 'md' | 'lg'
  onTint?: boolean
  className?: string
}

export function PurposeIcon({ code, size = 'md', onTint = false, className }: PurposeIconProps) {
  const { Icon } = purposeTheme(code)

  return (
    <span
      className={classNames(
        'purpose-theme',
        styles.icon,
        styles[size],
        onTint && styles.onTint,
        className,
      )}
      style={purposeStyle(code)}
      aria-hidden="true"
    >
      <Icon className={styles.glyph} strokeWidth={2} />
    </span>
  )
}
