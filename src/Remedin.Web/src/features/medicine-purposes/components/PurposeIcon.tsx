import { classNames } from '@/shared/ui/classNames'

import { purposeStyle, purposeTheme } from '../model/purposeTheme'

import styles from './PurposeIcon.module.css'

interface PurposeIconProps {
  /** Código da finalidade ("N") ou classe terapêutica ("N6A4"). */
  code: string | null | undefined
  size?: 'sm' | 'md' | 'lg'
  /**
   * Sobre um fundo já na cor da finalidade, o círculo claro some no fundo.
   * Com esta opção ele fica branco e continua visível.
   */
  onTint?: boolean
  className?: string
}

/**
 * O ícone da finalidade num círculo da cor dela. Decorativo: o nome da
 * finalidade está sempre escrito ao lado.
 *
 * Ícones da biblioteca Lucide (lucide.dev), licença ISC.
 */
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
