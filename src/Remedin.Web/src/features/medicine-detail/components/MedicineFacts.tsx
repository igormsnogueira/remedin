import type { ReactNode } from 'react'

import type { MedicineDetail } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { classNames } from '@/shared/ui/classNames'

import type { PackageOption } from '../model/packages'

import styles from './MedicineFacts.module.css'

/**
 * As três respostas da ficha, logo abaixo do nome: para que serve, se precisa
 * de receita e o preço máximo.
 *
 * Um painel só, com uma linha por pergunta, como a ficha desenhada na home.
 * Antes cada resposta era uma seção com título grande e a resposta em letra
 * pequena embaixo, que é o contrário do que importa.
 */
interface MedicineFactsProps {
  medicine: MedicineDetail
  /** Embalagens vendidas em farmácia, para a faixa de preço. */
  packages: PackageOption[]
  selected: PackageOption | null
}

export function MedicineFacts({ medicine, packages, selected }: MedicineFactsProps) {
  return (
    <dl className={styles.facts}>
      <Fact label="Para que serve">
        <Purpose medicine={medicine} />
      </Fact>

      <Fact label="Receita">
        <Prescription medicine={medicine} />
      </Fact>

      <Fact label={`Preço máximo em ${medicine.state}`}>
        <SelectedPrice medicine={medicine} packages={packages} selected={selected} />
      </Fact>
    </dl>
  )
}

function Fact({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className={styles.fact}>
      <dt className={styles.label}>{label}</dt>
      <dd className={styles.value}>{children}</dd>
    </div>
  )
}

/**
 * Sem tradução da classe terapêutica, mostra a classificação como a fonte
 * escreve, avisando que é termo técnico. Esconder jogaria fora a única
 * informação disponível sobre a finalidade.
 */
function Purpose({ medicine }: { medicine: MedicineDetail }) {
  if (medicine.purpose) {
    return <p className={styles.answer}>{medicine.purpose}</p>
  }

  if (medicine.therapeuticClassName) {
    return (
      <>
        <p className={styles.answer}>{medicine.therapeuticClassName}</p>
        <p className={styles.note}>
          Nome técnico da classificação oficial. Pergunte ao farmacêutico o que significa no seu
          caso.
        </p>
      </>
    )
  }

  return <p className={styles.unknown}>A fonte não informa.</p>
}

/**
 * Três casos, e não dois. `requiresPrescription` nulo significa que a fonte não
 * publicou a tarja — mostrar isso como venda livre seria a interface afirmando
 * o que o dado não diz, sobre a informação que pode fazer a pessoa sair de casa
 * para nada.
 */
function Prescription({ medicine }: { medicine: MedicineDetail }) {
  if (medicine.requiresPrescription === null) {
    return (
      <>
        <p className={styles.unknown}>A fonte não informa.</p>
        <p className={styles.note}>Confirme na farmácia antes de ir.</p>
      </>
    )
  }

  return (
    <>
      <p
        className={classNames(
          styles.badge,
          medicine.requiresPrescription ? styles.badgeRequired : styles.badgeFree,
        )}
      >
        {medicine.prescriptionRule ?? (medicine.requiresPrescription ? 'Exige receita' : 'Não precisa')}
      </p>
      {medicine.requiresPrescription && (
        <p className={styles.note}>A farmácia só vende com a receita em mãos, dentro da validade.</p>
      )}
    </>
  )
}

/**
 * O preço da embalagem escolhida, e não uma faixa: "de R$ 6 a R$ 20" não
 * responde quanto a pessoa vai pagar. A faixa fica como nota, para mostrar
 * que existem outras.
 */
function SelectedPrice({ medicine, packages, selected }: MedicineFactsProps) {
  if (!selected || selected.price === null) {
    return (
      <p className={styles.unknown}>
        {medicine.soldInPharmacy ? 'Preço não publicado.' : 'Não é vendido em farmácia.'}
      </p>
    )
  }

  const prices = packages
    .map((option) => option.price)
    .filter((price): price is number => price !== null)
  const lowest = Math.min(...prices)
  const highest = Math.max(...prices)

  return (
    <>
      <p className={styles.priceLine}>
        <span className={`${styles.price} numeric`}>{formatPrice(selected.price)}</span>
        <span className={styles.priceFor}>{selected.title}</span>
      </p>

      {selected.perUnit !== null && selected.showPerUnit && (
        <p className={`${styles.note} numeric`}>
          {formatPrice(selected.perUnit)} por {selected.unitLabel}
        </p>
      )}

      <p className={styles.note}>
        {packages.length > 1 && (
          <>
            <a href="#embalagens">
              {packages.length} embalagens, de {formatPrice(lowest)} a {formatPrice(highest)}
            </a>
            .{' '}
          </>
        )}
        Teto da CMED: a farmácia pode cobrar menos, nunca mais.
      </p>
    </>
  )
}
