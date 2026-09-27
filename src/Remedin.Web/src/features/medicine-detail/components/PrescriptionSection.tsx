import type { MedicineDetail } from '@/shared/api/types'
import { Notice } from '@/shared/ui/Notice/Notice'

/**
 * Exige receita ou não.
 *
 * Três casos, e não dois. `requiresPrescription` nulo significa que a fonte não
 * publicou a tarja — mostrar isso como "venda livre" seria a interface
 * afirmando o que o dado não diz, sobre a única informação da ficha que pode
 * fazer a pessoa sair de casa para nada.
 */
export function PrescriptionSection({ medicine }: { medicine: MedicineDetail }) {
  return (
    <section>
      <h2>Precisa de receita?</h2>

      {medicine.requiresPrescription === null ? (
        <Notice tone="warning">
          A fonte não informa a tarja deste produto. Confirme na farmácia antes de ir.
        </Notice>
      ) : (
        <>
          <p>{medicine.prescriptionRule}</p>
          {medicine.requiresPrescription && (
            <Notice>
              A farmácia só pode vender com a receita em mãos, dentro do prazo de validade dela.
            </Notice>
          )}
        </>
      )}
    </section>
  )
}
