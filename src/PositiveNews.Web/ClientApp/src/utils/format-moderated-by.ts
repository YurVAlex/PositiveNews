/** Formats moderator id for admin tables; null/undefined becomes "No". */
export function formatModeratedBy(value: number | null | undefined): string {
  return value != null ? String(value) : 'No'
}
