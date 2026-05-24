import { describe, expect, it } from 'vitest'
import { formatApiUtcAsLocal, parseApiUtcDate } from './format-api-datetime'

describe('format-api-datetime', () => {
  it('parses bare ISO timestamps as UTC', () => {
    const date = parseApiUtcDate('2026-01-15T12:00:00')

    expect(date?.toISOString()).toBe('2026-01-15T12:00:00.000Z')
  })

  it('parses Z-suffixed timestamps as UTC', () => {
    const date = parseApiUtcDate('2026-01-15T12:00:00Z')

    expect(date?.toISOString()).toBe('2026-01-15T12:00:00.000Z')
  })

  it('returns em dash for empty values', () => {
    expect(formatApiUtcAsLocal(null)).toBe('—')
    expect(formatApiUtcAsLocal('')).toBe('—')
  })
})
