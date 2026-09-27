import type { MedicineDetail } from '@/shared/api/types'
import { Tag } from '@/shared/ui/Tag/Tag'

import styles from './MedicineHeader.module.css'

export function MedicineHeader({ medicine }: { medicine: MedicineDetail }) {
  return (
    <header className={styles.header}>
      <h1>{medicine.name}</h1>

      {medicine.manufacturer && <p className={styles.manufacturer}>{medicine.manufacturer}</p>}

      <p className={styles.registration}>
        Registro na ANVISA{' '}
        <span className="numeric">{medicine.registrationNumber}</span>
      </p>

      <div className={styles.tags}>
        {medicine.activeIngredient && <Tag>{medicine.activeIngredient}</Tag>}
        {!medicine.isActive && <Tag tone="muted">Registro inativo</Tag>}
        {!medicine.soldRecently && <Tag tone="muted">Sem venda registrada no último ano</Tag>}
      </div>
    </header>
  )
}
