import styles from './Spinner.module.css'

interface SpinnerProps {
  label: string
}

export function Spinner({ label }: SpinnerProps) {
  return (
    <span role="status">
      <span className={styles.spinner} />
      <span className="visually-hidden">{label}</span>
    </span>
  )
}
