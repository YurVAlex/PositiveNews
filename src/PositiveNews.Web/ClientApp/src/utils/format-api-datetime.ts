/**
 * Parses and displays UTC timestamps from the API in the user's local timezone.
 */

const localDateTimeOptions: Intl.DateTimeFormatOptions = {
  year: 'numeric',
  month: 'short',
  day: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
}

const hasTimezoneSuffix = (value: string) =>
  /[zZ]$|[+-]\d{2}:\d{2}$/.test(value)

/**
 * Parses API date strings as UTC. Bare ISO values without offset are treated as UTC
 * (matches server-stored UTC timestamps and nextRunAtUtc serialization).
 */
export function parseApiUtcDate(iso: string | null | undefined): Date | null {
  if (!iso) return null
  const trimmed = iso.trim()
  if (!trimmed) return null

  const normalized = hasTimezoneSuffix(trimmed) ? trimmed : `${trimmed}Z`
  const date = new Date(normalized)
  return Number.isNaN(date.getTime()) ? null : date
}

/** Formats a UTC API timestamp in the user's local timezone. */
export function formatApiUtcAsLocal(iso: string | null | undefined): string {
  const date = parseApiUtcDate(iso)
  if (!date) return '—'
  return date.toLocaleString(undefined, localDateTimeOptions)
}
