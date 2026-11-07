import { StateSelect } from '@/features/state-selection/components/StateSelect'
import type { MedicineDetail } from '@/shared/api/types'

import styles from './MedicineAside.module.css'

/**
 * Coluna lateral da ficha: o estado e as informações de referência.
 *
 * O estado fica aqui, e não junto da tabela de preços, porque muda todos os
 * valores da página, inclusive os das alternativas.
 */
export function MedicineAside({ medicine }: { medicine: MedicineDetail }) {
  return (
    <aside className={styles.aside} aria-label="Estado e referências">
      <StateSelect />

      <p className={styles.text}>
        O preço máximo muda com o ICMS de cada estado. Em {medicine.state}, a alíquota é de{' '}
        {medicine.icmsRate}%.
      </p>

      <dl className={styles.reference}>
        <div>
          <dt>Registro na ANVISA</dt>
          <dd className="numeric">{medicine.registrationNumber}</dd>
        </div>
        <div>
          <dt>Fonte dos preços</dt>
          <dd>Câmara de Regulação do Mercado de Medicamentos (CMED)</dd>
        </div>
      </dl>
    </aside>
  )
}
