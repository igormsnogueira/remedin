import { useId } from 'react'

import { formatPrice } from '@/shared/format/currency'
import { classNames } from '@/shared/ui/classNames'

import type { PackageOption } from '../model/packages'

import styles from './PackagesSection.module.css'

interface PackagesSectionProps {
  pharmacy: PackageOption[]
  hospital: PackageOption[]
  selectedKey: string | null
  onSelect: (key: string) => void
}

/**
 * As embalagens à venda, para escolher uma.
 *
 * A escolha muda o preço do topo e a comparação com os equivalentes. É a
 * ideia de "sua receita" dos comparadores de preço: primeiro se diz o que vai
 * comprar, depois se compara a mesma coisa.
 *
 * Botões de rádio de verdade, e não cartões clicáveis: assim as setas do
 * teclado trocam a opção e o leitor de tela diz "1 de 4, selecionado".
 */
export function PackagesSection({ pharmacy, hospital, selectedKey, onSelect }: PackagesSectionProps) {
  const name = useId()

  if (pharmacy.length === 0 && hospital.length === 0) {
    return null
  }

  return (
    <section className={styles.section} id="embalagens" aria-labelledby="packages-title">
      <h2 className={styles.title} id="packages-title">
        Escolha a embalagem
      </h2>

      {pharmacy.length > 0 && (
        <fieldset className={styles.list}>
          <legend className="visually-hidden">Embalagem</legend>

          {pharmacy.map((option) => (
            <label
              key={option.key}
              className={classNames(styles.row, option.key === selectedKey && styles.selected)}
            >
              <input
                className={styles.radio}
                type="radio"
                name={name}
                value={option.key}
                checked={option.key === selectedKey}
                onChange={() => onSelect(option.key)}
              />

              <span className={styles.description}>{option.title}</span>

              <Price option={option} />
            </label>
          ))}
        </fieldset>
      )}

      {/* Recolhido: não vai ao balcão, e só interessa a quem trabalha na área. */}
      {hospital.length > 0 && (
        <details className={styles.hospital}>
          <summary>Uso restrito a hospital ({hospital.length})</summary>
          <p className={styles.hospitalNote}>
            Não são vendidas em farmácia. O preço publicado é o de fábrica.
          </p>
          <ul className={styles.plainList}>
            {hospital.map((option) => (
              <li key={option.key} className={styles.row}>
                <span className={styles.description}>{option.title}</span>
                <Price option={option} />
              </li>
            ))}
          </ul>
        </details>
      )}
    </section>
  )
}

function Price({ option }: { option: PackageOption }) {
  if (option.price === null) {
    return <span className={styles.missing}>preço não publicado</span>
  }

  return (
    <span className={styles.price}>
      <span className={`${styles.amount} numeric`}>{formatPrice(option.price)}</span>
      {option.perUnit !== null && option.showPerUnit && (
        <span className={`${styles.unit} numeric`}>
          {formatPrice(option.perUnit)} por {option.unitLabel}
        </span>
      )}
    </span>
  )
}
