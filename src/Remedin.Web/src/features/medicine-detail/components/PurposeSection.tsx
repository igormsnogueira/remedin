import type { MedicineDetail } from '@/shared/api/types'
import { Notice } from '@/shared/ui/Notice/Notice'

/**
 * Para que serve.
 *
 * Sem tradução da classe terapêutica, mostra a classificação como a fonte
 * escreve, avisando que é termo técnico. Esconder jogaria fora a única
 * informação disponível sobre a finalidade.
 */
export function PurposeSection({ medicine }: { medicine: MedicineDetail }) {
  if (!medicine.purpose && !medicine.therapeuticClassName) {
    return null
  }

  return (
    <section>
      <h2>Para que serve</h2>

      {medicine.purpose ? (
        <p>{medicine.purpose}</p>
      ) : (
        <Notice>
          Classificação da fonte, em termo técnico: {medicine.therapeuticClassName}. Pergunte ao
          farmacêutico o que isso significa no seu caso.
        </Notice>
      )}
    </section>
  )
}
