import type { MedicineAlternative } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'

import styles from './SavingsHighlight.module.css'

interface SavingsHighlightProps {
  savingsPerUnit: number
  cheapest: MedicineAlternative
}

/**
 * A economia por comprimido.
 *
 * O número vem pronto da API, que só o calcula entre embalagens de mesma
 * dosagem com as duas quantidades conhecidas. A tela não recalcula nem
 * completa nada: qualquer conta feita aqui seria uma segunda versão da regra.
 */
export function SavingsHighlight({ savingsPerUnit, cheapest }: SavingsHighlightProps) {
  return (
    <div className={styles.highlight}>
      <p>
        Economia de <span className={`${styles.amount} numeric`}>{formatPrice(savingsPerUnit)}</span>{' '}
        por unidade com {cheapest.name}
        {cheapest.manufacturer && `, da ${cheapest.manufacturer}`}.
      </p>
      <p className={styles.detail}>
        Mesma dosagem do medicamento consultado. {cheapest.presentation}.
      </p>
    </div>
  )
}
