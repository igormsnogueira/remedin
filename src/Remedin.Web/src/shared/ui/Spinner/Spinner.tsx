import styles from './Spinner.module.css'

interface SpinnerProps {
  /** O que está sendo carregado, para quem usa leitor de tela. */
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
