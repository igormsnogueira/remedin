// Instanciado uma vez: Intl.NumberFormat é caro de construir, e a lista de
// resultados chamaria o construtor a cada linha.
const brl = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

export function formatPrice(value: number): string {
  return brl.format(value)
}
