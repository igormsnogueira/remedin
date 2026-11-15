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

const FALLBACK: PurposeTheme = { hue: 165, Icon: Pill }

export function purposeTheme(code: string | null | undefined): PurposeTheme {
  const group = code?.charAt(0).toUpperCase()
  return (group && THEMES[group]) || FALLBACK
}

export function purposeStyle(code: string | null | undefined): CSSProperties {
  return { '--purpose-hue': purposeTheme(code).hue } as CSSProperties
}
