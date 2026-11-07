/**
 * Descrição de embalagem da CMED em português legível.
 *
 * A fonte escreve "10 MG COM REV CT BL AL PLAS TRANS X 10", abreviado para
 * quem é da área. A pessoa quer saber três coisas: o que vem (comprimido,
 * xarope), quanto vem e a dosagem. O resto (cartucho, blister, tipo de
 * plástico) não muda a escolha dela, e fica de fora.
 *
 * Quando a forma farmacêutica não é reconhecida, devolve nulo e a tela mostra
 * o texto original. Uma tradução errada seria pior que nenhuma.
 */

export interface ReadablePresentation {
  /** "10 comprimidos revestidos", "Xarope, frasco de 120 ml". */
  content: string
  /** "10 mg", "5 mg/ml". Nulo quando a descrição não começa pela dosagem. */
  strength: string | null
  /**
   * Dosagem em miligramas, quando é uma só e por unidade ("10 MG"). Nula em
   * concentrações ("5 MG/ML") e associações ("10 MG + 5 MG"), que não se
   * comparam com um número só.
   */
  milligrams: number | null
  /**
   * Quantas unidades vêm e o nome de uma ("comprimido"), para o preço por
   * unidade. Nulo nas formas medidas, como xarope, e quando a quantidade não
   * aparece na descrição.
   */
  units: { count: number; name: string } | null
  /**
   * Quanto vem nas formas medidas ("frasco de 120 ml" é 120 ml; "50 frascos
   * de 120 ml" é 6000 ml), para o preço por ml ou por grama. Nulo nas formas
   * contadas e quando o volume não aparece.
   */
  measure: { amount: number; unit: 'ml' | 'g' } | null
  /**
   * Liberação modificada (prolongada ou retardada). Não se troca por um de
   * liberação comum, mesmo com a mesma dosagem: muda a dose diária e o
   * horário.
   */
  release: 'prolonged' | 'delayed' | 'normal'
}

interface Form {
  /** Abreviações como aparecem na fonte, em sequência. */
  tokens: string[]
  singular: string
  plural: string
  /** Contável (comprimido) ou medido (xarope, em ml). */
  countable: boolean
}

// Da mais longa para a mais curta: "COM REV" precisa ser testado antes de
// "COM", senão todo comprimido revestido vira comprimido simples.
const FORMS: Form[] = [
  form('COM REV', 'comprimido revestido', 'comprimidos revestidos'),
  form('COM EFEV', 'comprimido efervescente', 'comprimidos efervescentes'),
  form('COM MAST', 'comprimido mastigável', 'comprimidos mastigáveis'),
  form('COM ORODISP', 'comprimido que dissolve na boca', 'comprimidos que dissolvem na boca'),
  form('COM', 'comprimido', 'comprimidos'),
  form('CAP GEL MOLE', 'cápsula mole', 'cápsulas moles'),
  form('CAP MOLE', 'cápsula mole', 'cápsulas moles'),
  form('CAP DURA', 'cápsula', 'cápsulas'),
  form('CAP', 'cápsula', 'cápsulas'),
  form('DRG', 'drágea', 'drágeas'),
  form('SUP', 'supositório', 'supositórios'),
  form('OVL', 'óvulo', 'óvulos'),
  form('ADES', 'adesivo', 'adesivos'),
  form('PAST', 'pastilha', 'pastilhas'),
  measured('PO LIOF SOL INJ', 'Pó para solução injetável'),
  measured('PO SUS OR', 'Pó para suspensão oral'),
  measured('PO SOL OR', 'Pó para solução oral'),
  measured('XPE', 'Xarope'),
  measured('ELX', 'Elixir'),
  measured('SOL OR', 'Solução oral'),
  measured('SOL INJ', 'Solução injetável'),
  measured('SOL OFT', 'Colírio'),
  measured('SOL NAS', 'Solução nasal'),
  measured('SOL TOP', 'Solução de uso na pele'),
  measured('SUS OR', 'Suspensão oral'),
  measured('SUS INJ', 'Suspensão injetável'),
  measured('SUS OFT', 'Colírio'),
  measured('EMU', 'Emulsão'),
  measured('CREM', 'Creme'),
  measured('POM', 'Pomada'),
  measured('GEL OR', 'Gel oral'),
  measured('GEL', 'Gel'),
  measured('LOC', 'Loção'),
  measured('XAMP', 'Xampu'),
  measured('SPR', 'Spray'),
  measured('AER', 'Aerossol'),
]

const RELEASE_TEXT = {
  prolonged: 'de liberação prolongada',
  delayed: 'de liberação retardada',
} as const

/** Recipiente das formas medidas, para "frasco de 120 ml". */
const CONTAINERS: Record<string, string> = {
  FR: 'frasco',
  BG: 'bisnaga',
  TB: 'tubo',
  AMP: 'ampola',
  FA: 'frasco-ampola',
  SER: 'seringa',
  ENV: 'envelope',
}

export function describePresentation(raw: string): ReadablePresentation | null {
  // A fonte é irregular no espaçamento: "5MG/ML", "5 MG / ML", "MG  COM".
  const text = raw
    .toUpperCase()
    .replace(/\s*\/\s*/g, '/')
    .replace(/(\d)(MG|MCG|ML|G|UI)\b/g, '$1 $2')
    .replace(/\s+/g, ' ')
    .trim()

  const words = text.split(' ')
  const match = findForm(words)

  if (!match) {
    return null
  }

  const prefix = words.slice(0, match.index).join(' ')
  const rest = words.slice(match.index + match.form.tokens.length).join(' ')
  const count = match.form.countable ? lastCount(rest) : null

  // A fonte escreve "LIB PROL", "LIB PROLONG", "LIB RETARD". Só o começo da
  // palavra é estável.
  const release = /\bLIB PROL/.test(text) ? 'prolonged' : /\bLIB RETARD/.test(text) ? 'delayed' : 'normal'
  const content = match.form.countable
    ? describeCount(match.form, rest)
    : describeMeasure(match.form, rest)

  return {
    content: release === 'normal' ? content : `${content} ${RELEASE_TEXT[release]}`,
    release,
    measure: match.form.countable ? null : readMeasure(rest),
    strength: readStrength(prefix),
    milligrams: readMilligrams(prefix),
    // "comprimido revestido" vira "comprimido": é assim que se fala do preço
    // de cada um.
    units: count ? { count, name: match.form.singular.split(' ')[0] ?? match.form.singular } : null,
  }
}

/**
 * O volume total de uma forma medida. Só antes do "+", que é o acessório
 * (copo medidor), e multiplicado pelo número de frascos quando há mais de um.
 */
function readMeasure(rest: string): { amount: number; unit: 'ml' | 'g' } | null {
  const main = rest.split('+')[0] ?? ''
  const volume = /X ([\d.,]+) (ML|G|L)\b/.exec(main)

  if (!volume?.[1] || !volume[2]) {
    return null
  }

  let amount = Number(volume[1].replace(/\./g, '').replace(',', '.'))
  let unit: 'ml' | 'g' = volume[2] === 'G' ? 'g' : 'ml'

  if (volume[2] === 'L') {
    amount *= 1000
    unit = 'ml'
  }

  const container = main.split(' ').find((word) => word in CONTAINERS)
  const count = container ? Number(new RegExp(`(\\d+) ${container}\\b`).exec(main)?.[1] ?? 1) : 1

  return amount > 0 ? { amount: amount * count, unit } : null
}

/** "10 MG" vira 10 e "1 G" vira 1000. Concentração e associação ficam de fora. */
function readMilligrams(prefix: string): number | null {
  const single = /^([\d,.]+) (MG|G)$/.exec(prefix.trim())

  if (!single?.[1]) {
    return null
  }

  const value = Number(single[1].replace(/\./g, '').replace(',', '.'))
  return single[2] === 'G' ? value * 1000 : value
}

function describeCount(form: Form, rest: string): string {
  // A quantidade é o último "X N" da descrição: "CT BL AL X 30".
  const count = lastCount(rest)

  if (count === null) {
    return capitalize(form.plural)
  }

  return `${count} ${count === 1 ? form.singular : form.plural}`
}

function describeMeasure(form: Form, rest: string): string {
  // Só a parte antes de um "+": o que vem depois é acessório, como o copo
  // medidor de "X 120 ML + COP X 10 ML".
  const main = rest.split('+')[0] ?? ''

  const volumeMatch = /X ([\d,.]+) (ML|G|L)\b/.exec(main)
  const volume = volumeMatch ? `${volumeMatch[1]} ${volumeMatch[2]?.toLowerCase()}` : null

  const container = main.split(' ').find((word) => word in CONTAINERS)
  const name = container ? CONTAINERS[container] : undefined

  // "CT 4 SER" e "CX 50 FR": mais de um recipiente na mesma embalagem.
  const count = container ? new RegExp(`(\\d+) ${container}\\b`).exec(main)?.[1] : undefined
  const quantity = name && count && count !== '1' ? `${count} ${plural(name)}` : name

  // Solução oral que sai em gotas é o que a pessoa conhece como "gotas".
  const drops = /\bGOT\b/.test(main) && form.singular === 'Solução oral'
  const parts: string[] = [drops ? `${form.singular} em gotas` : form.singular]

  const amount = quantity && volume ? `${quantity} de ${volume}` : (volume ?? quantity)

  if (amount) {
    parts.push(amount)
  }

  return parts.join(', ')
}

function findForm(words: string[]): { form: Form; index: number } | null {
  for (const candidate of FORMS) {
    for (let index = 0; index + candidate.tokens.length <= words.length; index++) {
      if (candidate.tokens.every((token, offset) => words[index + offset] === token)) {
        return { form: candidate, index }
      }
    }
  }

  return null
}

/** "10 MG", "5 MG/ML", "10 MG/G + 0,443 MG/G" viram minúsculas. */
function readStrength(prefix: string): string | null {
  const strength = prefix.trim()

  if (!/^[\d,.]+ (MG|MCG|G|UI|%)/.test(strength)) {
    return null
  }

  return strength.replace(/\b(MG|MCG|ML|G|UI)\b/g, (unit) => unit.toLowerCase())
}

function lastCount(text: string): number | null {
  const counts = [...text.matchAll(/X (\d+)\b(?! ?(ML|G|L)\b)/g)]
  const last = counts.at(-1)
  return last ? Number(last[1]) : null
}

function form(tokens: string, singular: string, pluralForm: string): Form {
  return { tokens: tokens.split(' '), singular, plural: pluralForm, countable: true }
}

function measured(tokens: string, name: string): Form {
  return { tokens: tokens.split(' '), singular: name, plural: name, countable: false }
}

/** "frasco-ampola" vira "frascos-ampola": o plural vai na primeira palavra. */
function plural(word: string): string {
  const [head, ...tail] = word.split('-')
  return [`${head}s`, ...tail].join('-')
}

function capitalize(text: string): string {
  return text.charAt(0).toUpperCase() + text.slice(1)
}
