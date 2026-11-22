export function parsePositiveId(value: unknown): number {
  if (typeof value !== 'string' || !/^[1-9]\d*$/.test(value)) return 0

  const id = Number(value)
  return Number.isSafeInteger(id) ? id : 0
}
