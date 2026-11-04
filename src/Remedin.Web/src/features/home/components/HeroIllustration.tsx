import type { CSSProperties } from 'react'

import styles from './HeroIllustration.module.css'

/** Posição das cápsulas na cartela. A casa da direita fica vazia, já usada. */
const BLISTER = [
  { x: 56, y: 214 },
  { x: 96, y: 214 },
  { x: 56, y: 252 },
  { x: 96, y: 252 },
]

/**
 * Ilustração da home: uma ficha de remédio sobre um painel da cor da marca.
 *
 * A ficha mostra as três respostas que o site dá — para que serve, se precisa
 * de receita e o preço máximo —, então a imagem explica o produto em vez de só
 * decorar. Os valores são ilustrativos e sem nome de produto, para não serem
 * lidos como dado real.
 *
 * Decorativa para o leitor de tela: o mesmo conteúdo já está escrito no texto
 * ao lado.
 *
 * Na abertura, a ficha é montada na ordem em que a pessoa vai ler: cartão,
 * respostas, preço e o selo. A animação fica no CSS e não roda para quem pediu
 * menos movimento ao sistema.
 */
export function HeroIllustration({ className }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 360 300"
      role="presentation"
      aria-hidden="true"
      focusable="false"
    >
      <defs>
        <clipPath id="hero-panel">
          <rect width="360" height="300" rx="24" />
        </clipPath>
      </defs>

      <g clipPath="url(#hero-panel)">
        <rect width="360" height="300" fill="var(--color-accent)" />

        {/* Cápsulas grandes cortadas pela borda: textura sem degradê. */}
        <g opacity="0.18">
          <Capsule cx={330} cy={40} size={3.2} angle={-38} />
          <Capsule cx={20} cy={270} size={2.6} angle={-38} />
        </g>

        {/* A cartela aparece por trás da ficha. O grupo de fora anima, o de
            dentro posiciona: o transform do CSS substituiria o do atributo. */}
        <g className={styles.blister}>
          <g transform="rotate(14 92 236)">
            <rect x="26" y="186" width="132" height="94" rx="14" fill="var(--color-surface)" />
            {BLISTER.map(({ x, y }) => (
              <Capsule key={`${x}-${y}`} cx={x} cy={y} />
            ))}
            <rect x="116" y="236" width="30" height="30" rx="9" fill="var(--color-border-soft)" />
          </g>
        </g>
      </g>

      <g className={styles.card}>
        <g transform="rotate(-4 190 150)">
          <rect x="74" y="40" width="232" height="214" rx="16" fill="var(--color-surface)" />

          <Capsule cx={104} cy={72} size={0.9} />
          <rect x="124" y="62" width="104" height="10" rx="5" fill="var(--color-heading)" />
          <rect x="124" y="78" width="64" height="7" rx="3.5" fill="var(--color-border)" />

          <Row y={104} label="Para que serve" value="Dor e febre" order={0} />
          <Row y={152} label="Receita" value="Não precisa" order={1} />

          <g className={styles.price}>
            <line x1="94" x2="286" y1="196" y2="196" stroke="var(--color-border-soft)" strokeWidth="1.5" />
            <text x="94" y="218" fill="var(--color-text-muted)" fontSize="11" fontFamily="inherit">
              Preço máximo
            </text>
            <text
              x="94"
              y="240"
              fill="var(--color-accent)"
              fontSize="20"
              fontWeight="700"
              fontFamily="inherit"
            >
              até R$ 9,90
            </text>
          </g>
        </g>
      </g>

      {/* Selo de conferido, sobre o canto da ficha. */}
      <g className={styles.badge}>
        <circle cx="300" cy="52" r="20" fill="var(--color-accent-light)" />
        <path
          className={styles.check}
          d="M291 52 l6 6 l12 -12"
          fill="none"
          stroke="var(--color-heading)"
          strokeWidth="3.5"
          strokeLinecap="round"
          strokeLinejoin="round"
          pathLength={1}
        />
      </g>
    </svg>
  )
}

function Row({
  y,
  label,
  value,
  order,
}: {
  y: number
  label: string
  value: string
  order: number
}) {
  return (
    // A ordem vira atraso no CSS, para as linhas entrarem uma depois da outra.
    <g className={styles.row} style={{ '--order': order } as CSSProperties}>
      <line x1="94" x2="286" y1={y} y2={y} stroke="var(--color-border-soft)" strokeWidth="1.5" />
      <text x="94" y={y + 20} fill="var(--color-text-muted)" fontSize="11" fontFamily="inherit">
        {label}
      </text>
      <rect x="94" y={y + 27} width={value.length * 7.4 + 20} height="20" rx="10" fill="var(--color-accent-soft)" />
      <text
        x="104"
        y={y + 41}
        fill="var(--color-accent-soft-text)"
        fontSize="12"
        fontWeight="700"
        fontFamily="inherit"
      >
        {value}
      </text>
    </g>
  )
}

/** A mesma cápsula do logo, em qualquer tamanho. */
function Capsule({
  cx,
  cy,
  size = 1,
  angle = -38,
}: {
  cx: number
  cy: number
  size?: number
  angle?: number
}) {
  return (
    <g transform={`translate(${cx} ${cy}) rotate(${angle}) scale(${size})`}>
      <rect x="-15" y="-7" width="30" height="14" rx="7" fill="var(--color-accent)" />
      <path d="M0 -7 h8 a7 7 0 0 1 0 14 h-8 z" fill="var(--color-accent-light)" />
      <rect x="-0.8" y="-7" width="1.6" height="14" fill="var(--color-surface)" />
    </g>
  )
}
