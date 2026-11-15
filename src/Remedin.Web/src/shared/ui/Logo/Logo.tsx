import styles from './Logo.module.css'

export function Logo() {
  return (
    <span className={styles.logo}>
      <LogoMark />
      <span className={styles.wordmark}>remedin</span>
    </span>
  )
}

export function LogoMark({ size = 30 }: { size?: number }) {
  return (
    <svg
      className={styles.mark}
      width={size}
      height={size}
      viewBox="0 0 32 32"
      aria-hidden="true"
      focusable="false"
    >
      <g transform="rotate(-38 16 16)">
        <rect x="3" y="10" width="26" height="12" rx="6" fill="var(--color-accent)" />
        <path d="M16 10h7a6 6 0 0 1 0 12h-7z" fill="var(--color-accent-light)" />
        <rect x="15.2" y="10" width="1.6" height="12" fill="var(--color-surface)" />
      </g>
    </svg>
  )
}
