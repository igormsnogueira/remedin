import { StateSelect } from '@/features/state-selection/components/StateSelect'
import type { MedicineDetail, PresentationDetail } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { Notice } from '@/shared/ui/Notice/Notice'

import styles from './PriceSection.module.css'

export function PriceSection({ medicine }: { medicine: MedicineDetail }) {
  const pharmacy = medicine.presentations.filter((item) => !item.hospitalOnly)
  const hospital = medicine.presentations.filter((item) => item.hospitalOnly)

  return (
    <section>
      <div className={styles.heading}>
        <h2>Preço máximo em {medicine.state}</h2>
        <StateSelect className={styles.state} />
      </div>

      {pharmacy.length === 0 ? (
        <Notice tone="warning">
          Nenhuma apresentação deste medicamento é vendida em farmácia.
        </Notice>
      ) : (
        <>
          <ul className={styles.list}>
            {pharmacy.map((presentation) => (
              <PresentationRow key={presentation.ggremCode} presentation={presentation} />
            ))}
          </ul>

          <Notice>
            Teto de venda ao consumidor definido pela CMED para {medicine.state}, com ICMS de{' '}
            {medicine.icmsRate}%. A farmácia pode cobrar menos, nunca mais.
          </Notice>
        </>
      )}

      {hospital.length > 0 && (
        <div className={styles.hospital}>
          <h3 className={styles.hospitalTitle}>Apresentações de uso restrito a hospital</h3>
          <Notice>
            Estas não vão ao balcão da farmácia, e o preço publicado é o de fábrica.
          </Notice>
          <ul className={styles.list}>
            {hospital.map((presentation) => (
              <PresentationRow key={presentation.ggremCode} presentation={presentation} hospital />
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}

function PresentationRow({
  presentation,
  hospital = false,
}: {
  presentation: PresentationDetail
  hospital?: boolean
}) {
  const price = hospital ? presentation.factoryPrice : presentation.consumerPrice

  return (
    <li className={styles.item}>
      <span className={styles.description}>{presentation.description}</span>

      {price === null ? (
        <span className={styles.missing}>preço não publicado</span>
      ) : (
        <span className={`${styles.amount} numeric`}>{formatPrice(price)}</span>
      )}
    </li>
  )
}
