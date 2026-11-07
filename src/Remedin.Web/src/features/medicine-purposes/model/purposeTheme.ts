import type { CSSProperties } from 'react'

import {
  Bone,
  Brain,
  Bug,
  Droplet,
  Eye,
  FlaskConical,
  Hand,
  HeartPulse,
  Hospital,
  Microscope,
  Pill,
  Ribbon,
  TestTube,
  Utensils,
  VenusAndMars,
  Wind,
} from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

/**
 * Cor e ícone de cada finalidade, para a pessoa reconhecer o tipo de remédio
 * antes de ler: vermelho é coração, roxo é sistema nervoso, azul é respiração.
 *
 * A cor é só um matiz (0 a 360). Claridade e saturação são as mesmas para
 * todas, definidas em .purpose-theme, então qualquer finalidade tem o mesmo
 * contraste de texto. Trocar a cor de uma é trocar um número.
 *
 * As chaves são a letra do grupo anatômico da classe terapêutica, a mesma
 * usada pela API em /finalidades (TherapeuticCategories, no domínio).
 */
interface PurposeTheme {
  hue: number
  Icon: LucideIcon
}

const THEMES: Record<string, PurposeTheme> = {
  A: { hue: 55, Icon: Utensils }, // Estômago, intestino e diabetes
  B: { hue: 10, Icon: Droplet }, // Sangue e circulação
  C: { hue: 25, Icon: HeartPulse }, // Coração e pressão
  D: { hue: 70, Icon: Hand }, // Pele
  G: { hue: 350, Icon: VenusAndMars }, // Saúde íntima e hormônios sexuais
  H: { hue: 305, Icon: FlaskConical }, // Hormônios
  J: { hue: 145, Icon: Microscope }, // Infecções
  K: { hue: 235, Icon: Hospital }, // Soluções hospitalares
  L: { hue: 330, Icon: Ribbon }, // Câncer e imunidade
  M: { hue: 85, Icon: Bone }, // Músculos, ossos e articulações
  N: { hue: 285, Icon: Brain }, // Sistema nervoso e dor
  P: { hue: 120, Icon: Bug }, // Vermes e parasitas
  R: { hue: 225, Icon: Wind }, // Respiração, gripe e alergia
  S: { hue: 200, Icon: Eye }, // Olhos e ouvidos
  T: { hue: 260, Icon: TestTube }, // Exames e diagnóstico
}

/** Sem classe conhecida: o verde da marca e uma cápsula. */
const FALLBACK: PurposeTheme = { hue: 165, Icon: Pill }

/**
 * Aceita o código da finalidade ("N") ou a classe terapêutica inteira
 * ("N6A4"): a primeira letra é o grupo nos dois casos.
 */
export function purposeTheme(code: string | null | undefined): PurposeTheme {
  const group = code?.charAt(0).toUpperCase()
  return (group && THEMES[group]) || FALLBACK
}

/**
 * O estilo que pinta um elemento com a cor da finalidade. Vai junto da classe
 * global purpose-theme, que deriva os tons a partir do matiz.
 */
export function purposeStyle(code: string | null | undefined): CSSProperties {
  return { '--purpose-hue': purposeTheme(code).hue } as CSSProperties
}
