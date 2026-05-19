import type { FeedSortParam } from '../api/articles-api'
import type { UserFeedPreferencesResponse } from '../api/types'

export const DEFAULT_MIN_POSITIVITY = 0.5
export const FEED_PREFS_DRAFT_KEY = 'positiveNews.feedPrefsDraft'

export type FeedPreferencesSnapshot = {
  topics: string[]
  sourceIds: number[]
  sort: FeedSortParam
  minPositivity: number
}

export function parsePage(raw: string | null): number {
  const n = Number(raw ?? '1')
  if (!Number.isFinite(n) || n < 1) {
    return 1
  }
  return Math.floor(n)
}

export function topicsFromSearchParams(searchParams: URLSearchParams): string[] {
  const ordered: string[] = []
  const seen = new Set<string>()
  for (const raw of searchParams.getAll('topic')) {
    const trimmed = raw.trim()
    if (!trimmed.length) continue
    const key = trimmed.toLowerCase()
    if (seen.has(key)) continue
    seen.add(key)
    ordered.push(trimmed)
  }
  return ordered
}

export function sourceIdsFromSearchParams(searchParams: URLSearchParams): number[] {
  const ordered: number[] = []
  const seen = new Set<number>()
  for (const raw of searchParams.getAll('source')) {
    const id = Number(raw)
    if (!Number.isInteger(id) || id < 1) continue
    if (seen.has(id)) continue
    seen.add(id)
    ordered.push(id)
  }
  return ordered
}

export function parseSort(raw: string | null): FeedSortParam {
  const value = raw?.toLowerCase()
  if (value === 'positivity') return 'positivity'
  if (value === 'preferences') return 'preferences'
  return 'date'
}

export function parseMinPositivity(raw: string | null): number {
  if (!raw?.trim()) {
    return DEFAULT_MIN_POSITIVITY
  }
  const n = Number(raw)
  if (!Number.isFinite(n)) {
    return DEFAULT_MIN_POSITIVITY
  }
  return Math.min(1, Math.max(0, n))
}

export function isSettingsOpen(searchParams: URLSearchParams): boolean {
  return searchParams.get('settings') === '1'
}

export function preferencesFromSearchParams(searchParams: URLSearchParams): FeedPreferencesSnapshot {
  return {
    topics: topicsFromSearchParams(searchParams),
    sourceIds: sourceIdsFromSearchParams(searchParams),
    sort: parseSort(searchParams.get('sort')),
    minPositivity: parseMinPositivity(searchParams.get('minPositivity')),
  }
}

export function snapshotToApiRequest(snapshot: FeedPreferencesSnapshot): UserFeedPreferencesResponse {
  return {
    topicNames: snapshot.topics,
    sourceIds: snapshot.sourceIds,
    minPositivity: snapshot.minPositivity,
    sortBy: snapshot.sort,
  }
}

export function preferencesFromApiResponse(response: UserFeedPreferencesResponse): FeedPreferencesSnapshot {
  return {
    topics: [...response.topicNames],
    sourceIds: [...response.sourceIds],
    sort: parseSort(response.sortBy),
    minPositivity: response.minPositivity,
  }
}

/** Keys that affect feed filtering and persisted preferences (excludes page and settings UI). */
export function preferenceKeysEqual(a: URLSearchParams, b: URLSearchParams): boolean {
  return serializePreferenceParams(a) === serializePreferenceParams(b)
}

export function serializePreferenceParams(params: URLSearchParams): string {
  const snapshot = preferencesFromSearchParams(params)
  const next = new URLSearchParams()
  applyPreferencesToSearchParams(next, snapshot, { includeSettings: false })
  return next.toString()
}

export function applyPreferencesToSearchParams(
  params: URLSearchParams,
  snapshot: FeedPreferencesSnapshot,
  options?: { includeSettings?: boolean; settingsOpen?: boolean; page?: number },
): URLSearchParams {
  params.delete('topic')
  params.delete('source')
  params.delete('sort')
  params.delete('minPositivity')

  snapshot.topics.forEach((t) => params.append('topic', t))
  snapshot.sourceIds.forEach((id) => params.append('source', String(id)))

  if (snapshot.sort !== 'date') {
    params.set('sort', snapshot.sort)
  }

  if (snapshot.minPositivity !== DEFAULT_MIN_POSITIVITY) {
    params.set('minPositivity', String(snapshot.minPositivity))
  }

  if (options?.page !== undefined) {
    params.set('page', String(options.page))
  }

  if (options?.includeSettings) {
    if (options.settingsOpen) {
      params.set('settings', '1')
    } else {
      params.delete('settings')
    }
  }

  return params
}

export function buildSearchFromSnapshot(
  snapshot: FeedPreferencesSnapshot,
  options?: { settingsOpen?: boolean; page?: number },
): string {
  const params = applyPreferencesToSearchParams(new URLSearchParams(), snapshot, {
    includeSettings: true,
    settingsOpen: options?.settingsOpen ?? false,
    page: options?.page ?? 1,
  })
  const qs = params.toString()
  return qs ? `?${qs}` : ''
}

export function saveFeedPrefsDraft(snapshot: FeedPreferencesSnapshot): void {
  try {
    sessionStorage.setItem(FEED_PREFS_DRAFT_KEY, JSON.stringify(snapshot))
  } catch {
    // ignore storage errors
  }
}

export function loadFeedPrefsDraft(): FeedPreferencesSnapshot | null {
  try {
    const raw = sessionStorage.getItem(FEED_PREFS_DRAFT_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as Partial<FeedPreferencesSnapshot>
    return {
      topics: Array.isArray(parsed.topics) ? parsed.topics.filter((t) => typeof t === 'string') : [],
      sourceIds: Array.isArray(parsed.sourceIds)
        ? parsed.sourceIds.filter((id) => Number.isInteger(id) && id > 0)
        : [],
      sort: parseSort(typeof parsed.sort === 'string' ? parsed.sort : null),
      minPositivity: typeof parsed.minPositivity === 'number'
        ? Math.min(1, Math.max(0, parsed.minPositivity))
        : DEFAULT_MIN_POSITIVITY,
    }
  } catch {
    return null
  }
}

export function clearFeedPrefsDraft(): void {
  try {
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  } catch {
    // ignore
  }
}

/**
 * Builds the feed path restoring guest/user prefs from navigation state or session draft.
 */
export function buildFeedReturnPath(feedSearch?: string | null): string {
  const trimmed = feedSearch?.trim()
  if (trimmed) {
    return trimmed.startsWith('?') ? `/${trimmed}` : `/?${trimmed}`
  }

  const draft = loadFeedPrefsDraft()
  if (draft) {
    return `/${buildSearchFromSnapshot(draft)}`
  }

  return '/'
}
