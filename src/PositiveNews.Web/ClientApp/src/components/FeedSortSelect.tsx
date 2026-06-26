/**
 * Feed sort dropdown and helpers for sort labels and preference hints.
 */
import type { FeedSortParam } from '../api/articles-api'


export type FeedSortSelectProps = {
  sortMode: FeedSortParam
  hasPreferences: boolean
  onSortChange: (sort: FeedSortParam) => void
  className?: string
}

/** Human-readable label for each feed sort mode. */
export function feedSortModeLabel(sort: FeedSortParam): string {
  if (sort === 'positivity') return 'positivity score'
  if (sort === 'preferences') return 'your preferences'
  return 'publication date'
}

/** Explains how preferred topics/sources affect ordering for the current sort mode. */
export function buildPreferenceSortHint(sortMode: FeedSortParam, sortLabel: string): string {
  if (sortMode === 'preferences') {
      return 'Matching preferred topics and sources are shown first, then by publication date.'
  }
  return `Matching preferred topics and sources are shown first, sorted by: ${sortLabel}.`
}

export function FeedSortSelect({ sortMode, hasPreferences, onSortChange, className }: FeedSortSelectProps) {
  return (
    <div className={['d-flex align-items-center gap-2', className].filter(Boolean).join(' ')}>
      <label htmlFor="feed-sort-select" className="small text-muted mb-0 text-nowrap">
        Sort by
      </label>
      <select
        id="feed-sort-select"
        className="form-select form-select-sm"
        style={{ width: 'auto', minWidth: '11rem' }}
        aria-label="Sort articles"
        value={sortMode}
        onChange={(e) => {
          const value = e.target.value
          if (value === 'positivity' || value === 'preferences' || value === 'date') {
            onSortChange(value)
          }
        }}
      >
        <option value="date">Publication date</option>
        <option value="positivity">Positivity score</option>
        <option value="preferences" disabled={!hasPreferences}>
          Your preferences
        </option>
      </select>
    </div>
  )
}