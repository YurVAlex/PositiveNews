/**
 * Feed filter/sort preferences as URL query params, sessionStorage drafts, and API payloads.
 * Keeps the home feed URL, browser session, and saved user settings in sync.
 */

import type { FeedSortParam } from '../api/articles-api'
import type { UserFeedPreferencesResponse } from '../api/types'

export const DEFAULT_MIN_POSITIVITY = 0
export const FEED_PREFS_DRAFT_KEY = 'positiveNews.feedPrefsDraft'
export const LAST_FEED_SEARCH_KEY = 'positiveNews.lastFeedSearch'
export const FEED_PREFS_LAST_SAVED_KEY = 'positiveNews.feedPrefsLastSaved'

type StoredLastSavedFeedPrefs = {
  userId: number
  serialized: string
}

/** React Router location for returning to the feed with restored query params. */
export type FeedReturnTo = {
  pathname: '/'
  search?: string
}

/** In-memory shape of feed filters shared by URL, session draft, and API. */
export type FeedPreferencesSnapshot = {
  topics: string[]
  sourceIds: number[]
  sort: FeedSortParam
  minPositivity: number
}

export const EMPTY_FEED_PREFERENCES: FeedPreferencesSnapshot = {
  topics: [],
  sourceIds: [],
  sort: 'date',
  minPositivity: DEFAULT_MIN_POSITIVITY,
}

/** Parses a page query value; falls back to 1 for missing or invalid input. */
export function parsePage(raw: string | null): number {
  const n = Number(raw ?? '1')
  if (!Number.isFinite(n) || n < 1) {
    return 1
  }
  return Math.floor(n)
}

/** Collects unique topic names from repeated `topic` query params, preserving order. */
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

/** Collects unique source IDs from repeated `source` query params, preserving order. */
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

/** Parses sort query value; unknown values default to date order. */
export function parseSort(raw: string | null): FeedSortParam {
  const value = raw?.toLowerCase()
  if (value === 'positivity') return 'positivity'
  if (value === 'preferences') return 'preferences'
  return 'date'
}

/** Parses minPositivity query value, clamped to [0, 1] with a default of 0. */
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

/** True when the feed settings panel should be open (`settings=1`). */
export function isSettingsOpen(searchParams: URLSearchParams): boolean {
  return searchParams.get('settings') === '1'
}

/** Builds a preference snapshot from the current URL search params. */
export function preferencesFromSearchParams(searchParams: URLSearchParams): FeedPreferencesSnapshot {
  return {
    topics: topicsFromSearchParams(searchParams),
    sourceIds: sourceIdsFromSearchParams(searchParams),
    sort: parseSort(searchParams.get('sort')),
    minPositivity: parseMinPositivity(searchParams.get('minPositivity')),
  }
}

/** Maps a client snapshot to the API request body for saving user feed preferences. */
export function snapshotToApiRequest(snapshot: FeedPreferencesSnapshot): UserFeedPreferencesResponse {
  return {
    topicNames: snapshot.topics,
    sourceIds: snapshot.sourceIds,
    minPositivity: snapshot.minPositivity,
    sortBy: snapshot.sort,
  }
}

/** Maps a saved API response back into the client preference snapshot shape. */
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

/** Serializes only preference-related query keys for comparison and storage fingerprints. */
export function serializePreferenceParams(params: URLSearchParams): string {
  const snapshot = preferencesFromSearchParams(params)
  return serializePreferenceSnapshot(snapshot)
}

/** Canonical serialized preference params (excludes page and settings). */
export function serializePreferenceSnapshot(snapshot: FeedPreferencesSnapshot): string {
  const next = new URLSearchParams()
  applyPreferencesToSearchParams(next, snapshot, { includeSettings: false })
  return next.toString()
}

/** Loads the last server-synced preference fingerprint for the given user, if any. */
export function loadLastSavedPreferenceParams(userId: number): string | null {
  try {
    const raw = sessionStorage.getItem(FEED_PREFS_LAST_SAVED_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as Partial<StoredLastSavedFeedPrefs>
    if (parsed.userId !== userId || typeof parsed.serialized !== 'string') {
      return null
    }
    return parsed.serialized
  } catch {
    return null
  }
}

/** Persists the canonical preference fingerprint after a successful server save. */
export function saveLastSavedPreferenceParams(userId: number, serialized: string): void {
  try {
    const payload: StoredLastSavedFeedPrefs = { userId, serialized }
    sessionStorage.setItem(FEED_PREFS_LAST_SAVED_KEY, JSON.stringify(payload))
  } catch {
    // ignore storage errors
  }
}

/** Clears the stored server-sync fingerprint (e.g. on logout). */
export function clearLastSavedPreferenceParams(): void {
  try {
    sessionStorage.removeItem(FEED_PREFS_LAST_SAVED_KEY)
  } catch {
    // ignore
  }
}

/** Writes snapshot fields into URLSearchParams, optionally including page and settings UI state. */
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

/** Builds a full feed query string (`?topic=…&page=…`) from a preference snapshot. */
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

/** Stores unsaved preference edits in sessionStorage while the settings panel is open. */
export function saveFeedPrefsDraft(snapshot: FeedPreferencesSnapshot): void {
  try {
    sessionStorage.setItem(FEED_PREFS_DRAFT_KEY, JSON.stringify(snapshot))
  } catch {
    // ignore storage errors
  }
}

/** Loads and sanitizes the session preference draft, or null when absent or corrupt. */
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

/** Removes the session preference draft. */
export function clearFeedPrefsDraft(): void {
  try {
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  } catch {
    // ignore
  }
}

/** Removes the stored last-visited feed URL from sessionStorage. */
export function clearLastFeedSearch(): void {
  try {
    sessionStorage.removeItem(LAST_FEED_SEARCH_KEY)
  } catch {
    // ignore
  }
}

/** Clears session-scoped feed preference storage (draft, last visit, and last server sync fingerprint). */
export function clearLocalFeedPreferences(): void {
  clearFeedPrefsDraft()
  clearLastFeedSearch()
  clearLastSavedPreferenceParams()
}

/** Builds default feed query params (no filters, page 1, optional settings panel). */
export function buildDefaultFeedSearchParams(options?: { settingsOpen?: boolean }): URLSearchParams {
  return applyPreferencesToSearchParams(new URLSearchParams(), EMPTY_FEED_PREFERENCES, {
    includeSettings: true,
    settingsOpen: options?.settingsOpen ?? false,
    page: 1,
  })
}

/** Persists the latest feed URL query (including page) for "Back to feed" when navigation state is lost. */
export function saveLastFeedSearch(feedSearch: string): void {
  const normalized = normalizeFeedSearch(feedSearch)
  try {
    if (normalized) {
      sessionStorage.setItem(LAST_FEED_SEARCH_KEY, normalized)
    } else {
      sessionStorage.removeItem(LAST_FEED_SEARCH_KEY)
    }
  } catch {
    // ignore storage errors
  }
}

/** Loads the last visited feed query string, normalized with a leading `?`. */
export function loadLastFeedSearch(): string | null {
  try {
    const raw = sessionStorage.getItem(LAST_FEED_SEARCH_KEY)
    if (!raw?.trim()) return null
    return normalizeFeedSearch(raw)
  } catch {
    return null
  }
}

function normalizeFeedSearch(feedSearch?: string | null): string {
  const trimmed = feedSearch?.trim()
  if (!trimmed) return ''
  return trimmed.startsWith('?') ? trimmed : `?${trimmed}`
}

/** True when the snapshot differs from the default feed (any active filter or non-default sort). */
export function hasNonDefaultPreferences(snapshot: FeedPreferencesSnapshot): boolean {
  return (
    snapshot.topics.length > 0 ||
    snapshot.sourceIds.length > 0 ||
    snapshot.sort !== 'date' ||
    snapshot.minPositivity !== DEFAULT_MIN_POSITIVITY
  )
}

/** True when the URL explicitly carries feed filter/sort preference keys. */
export function hasPreferenceParamsInUrl(params: URLSearchParams): boolean {
  if (params.has('topic') || params.has('source')) {
    return true
  }
  if (params.get('sort')?.trim()) {
    return true
  }
  if (params.get('minPositivity')?.trim()) {
    return true
  }
  return false
}

/**
 * Merges a session draft into current search params when the URL has no preference keys
 * (e.g. user returned via / or ?page=2 only).
 */
export function mergeDraftIntoSearchParams(
  current: URLSearchParams,
  draft: FeedPreferencesSnapshot,
): URLSearchParams {
  return applyPreferencesToSearchParams(new URLSearchParams(current), draft, {
    includeSettings: true,
    settingsOpen: isSettingsOpen(current),
    page: parsePage(current.get('page')),
  })
}

/** Whether landing search params should be hydrated from the session draft. */
export function shouldHydrateFeedFromDraft(params: URLSearchParams): boolean {
  if (hasPreferenceParamsInUrl(params)) {
    return false
  }
  const draft = loadFeedPrefsDraft()
  if (!draft || !hasNonDefaultPreferences(draft)) {
    return false
  }
  return serializePreferenceParams(params) !== serializePreferenceParams(
    applyPreferencesToSearchParams(new URLSearchParams(), draft),
  )
}

/**
 * When a stored feed URL only has page/settings (no topic/source/sort/minPositivity),
 * merge session preference draft so navbar "home" links do not drop filters.
 */
export function enrichFeedSearchWithDraft(search: string): string {
  const normalized = normalizeFeedSearch(search)
  if (!normalized) {
    return ''
  }

  const params = new URLSearchParams(normalized.startsWith('?') ? normalized.slice(1) : normalized)
  if (hasPreferenceParamsInUrl(params)) {
    return normalized
  }

  const draft = loadFeedPrefsDraft()
  if (!draft || !hasNonDefaultPreferences(draft)) {
    return normalized
  }

  const merged = mergeDraftIntoSearchParams(params, draft)
  const qs = merged.toString()
  return qs ? `?${qs}` : normalized
}

/**
 * Builds a feed route restoring query params (including page) from navigation state,
 * last visited feed URL, or session preference draft.
 */
export function buildFeedReturnTo(feedSearch?: string | null): FeedReturnTo {
  const fromState = normalizeFeedSearch(feedSearch)
  if (fromState) {
    const search = enrichFeedSearchWithDraft(fromState)
    return search ? { pathname: '/', search } : { pathname: '/' }
  }

  const fromLastVisit = loadLastFeedSearch()
  if (fromLastVisit) {
    const search = enrichFeedSearchWithDraft(fromLastVisit)
    return search ? { pathname: '/', search } : { pathname: '/' }
  }

  const draft = loadFeedPrefsDraft()
  if (draft) {
    const search = buildSearchFromSnapshot(draft)
    return search ? { pathname: '/', search } : { pathname: '/' }
  }

  return { pathname: '/' }
}

/** @deprecated Prefer {@link buildFeedReturnTo} for React Router links. */
export function buildFeedReturnPath(feedSearch?: string | null): string {
  const to = buildFeedReturnTo(feedSearch)
  return to.search ? `/${to.search}` : '/'
}
