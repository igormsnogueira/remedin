/** Junta classes ignorando as condicionais que não se aplicam. */
export function classNames(...values: Array<string | false | null | undefined>): string {
  return values.filter(Boolean).join(' ')
}
