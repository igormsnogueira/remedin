import { FavoriteButton } from '@/features/favorites/components/FavoriteButton'
import { PurposeIcon } from '@/features/medicine-purposes/components/PurposeIcon'
import { purposeStyle } from '@/features/medicine-purposes/model/purposeTheme'
import type { MedicineDetail } from '@/shared/api/types'
import { Tag } from '@/shared/ui/Tag/Tag'

import styles from './MedicineHeader.module.css'

/**
 * O topo da ficha, na cor da finalidade do remédio: dá para saber de que tipo
 * ele é antes de ler qualquer coisa.
 */
export function MedicineHeader({ medicine }: { medicine: MedicineDetail }) {
  return (
    <header
      className={`purpose-theme ${styles.header}`}
      style={purposeStyle(medicine.therapeuticClassCode)}
    >
      <div className={styles.title}>
        <PurposeIcon code={medicine.therapeuticClassCode} size="lg" onTint />

        {/* Mesmo nome de transição do cartão de onde a pessoa veio. */}
        <h1
          className={styles.name}
          style={{ viewTransitionName: `medicine-${medicine.registrationNumber}` }}
        >
          {medicine.name}
        </h1>

        <FavoriteButton medicine={medicine} />
      </div>

      {medicine.manufacturer && <p className={styles.manufacturer}>{medicine.manufacturer}</p>}

      <div className={styles.tags}>
        {medicine.activeIngredient && <Tag>{medicine.activeIngredient}</Tag>}
        {!medicine.isActive && <Tag tone="muted">Registro inativo</Tag>}
        {!medicine.soldRecently && <Tag tone="muted">Sem venda registrada no último ano</Tag>}
      </div>
    </header>
  )
}
