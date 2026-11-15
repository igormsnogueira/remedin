import type { ReactNode } from 'react'

import { ExternalLink } from 'lucide-react'

import type { MedicineDetail } from '@/shared/api/types'
import { formatPrice } from '@/shared/format/currency'
import { classNames } from '@/shared/ui/classNames'

import type { PackageOption } from '../model/packages'

import styles from './MedicineFacts.module.css'

interface MedicineFactsProps {
  medicine: MedicineDetail
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

      {medicine.leafletPublishedOn && (
        <Fact label="Bula">
          <Leaflet
            registrationNumber={medicine.registrationNumber}
            publishedOn={medicine.leafletPublishedOn}
          />
        </Fact>
      )}
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

function Prescription({ medicine }: { medicine: MedicineDetail }) {
  // Nulo é "a fonte não informa", e não "venda livre".
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

// O PDF do Bulário fica atrás de verificação contra robôs; o link leva à busca já filtrada pelo registro.
function Leaflet({
  registrationNumber,
  publishedOn,
}: {
  registrationNumber: string
  publishedOn: string
}) {
  const url = `https://consultas.anvisa.gov.br/#/bulario/q/?numeroRegistro=${registrationNumber}`

  return (
    <>
      <a className={styles.external} href={url} target="_blank" rel="noopener noreferrer">
        Abrir no site da ANVISA
        <ExternalLink className={styles.externalIcon} aria-hidden="true" />
        <span className="visually-hidden"> (abre em outra aba)</span>
      </a>
      <p className={styles.note}>
        Publicada em {formatDate(publishedOn)}. Na página da ANVISA, toque no ícone de PDF em
        &ldquo;Bula do Paciente&rdquo;.
      </p>
    </>
  )
}

// Sem Date: "2025-12-09" vira meia-noite UTC, que no Brasil ainda é dia 8.
function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-')
  return `${day}/${month}/${year}`
}
