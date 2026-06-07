import { describe, expect, it } from 'vitest'
import {
  applyPreferencesToSearchParams,
  buildFeedReturnTo,
  DEFAULT_MIN_POSITIVITY,
  FEED_PREFS_DRAFT_KEY,
  FEED_PREFS_LAST_SAVED_KEY,
  LAST_FEED_SEARCH_KEY,
  loadLastSavedPreferenceParams,
  saveLastSavedPreferenceParams,
  serializePreferenceSnapshot,
  hasNonDefaultPreferences,
  hasPreferenceParamsInUrl,
  mergeDraftIntoSearchParams,
  parseMinPositivity,
  parseSort,
  preferencesFromSearchParams,
  clearLocalFeedPreferences,
  saveFeedPrefsDraft,
  saveLastFeedSearch,
  serializePreferenceParams,
  shouldHydrateFeedFromDraft,
  topicsFromSearchParams,
} from './feed-preferences-url'

describe('feed-preferences-url', () => {
  it('parses topics and sort from search params', () => {
    const params = new URLSearchParams('topic=Health&topic=Science&sort=positivity')
    expect(topicsFromSearchParams(params)).toEqual(['Health', 'Science'])
    expect(parseSort(params.get('sort'))).toBe('positivity')
  })

  it('uses default min positivity when omitted', () => {
    const params = new URLSearchParams()
    expect(parseMinPositivity(params.get('minPositivity'))).toBe(DEFAULT_MIN_POSITIVITY)
  })

  it('serializes snapshot without default min positivity', () => {
    const params = applyPreferencesToSearchParams(new URLSearchParams(), {
      topics: ['Health'],
      sourceIds: [2],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    expect(params.get('minPositivity')).toBeNull()
    expect(params.getAll('topic')).toEqual(['Health'])
    expect(params.get('source')).toBe('2')
  })

  it('round-trips preferences through serialize', () => {
    const original = new URLSearchParams('topic=A&source=1&sort=preferences&minPositivity=0.7')
    const snapshot = preferencesFromSearchParams(original)
    const restored = applyPreferencesToSearchParams(new URLSearchParams(), snapshot)
    expect(serializePreferenceParams(restored)).toBe(serializePreferenceParams(original))
  })

  it('buildFeedReturnTo prefers navigation state over session draft', () => {
    saveFeedPrefsDraft({
      topics: ['Draft'],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    expect(buildFeedReturnTo('?topic=Health')).toEqual({ pathname: '/', search: '?topic=Health' })
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })

  it('buildFeedReturnTo preserves page from navigation state', () => {
    expect(buildFeedReturnTo('?page=3&topic=Health')).toEqual({
      pathname: '/',
      search: '?page=3&topic=Health',
    })
  })

  it('buildFeedReturnTo merges session draft into page-only last feed search', () => {
    saveFeedPrefsDraft({
      topics: ['Science'],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    saveLastFeedSearch('?page=3')
    expect(buildFeedReturnTo()).toEqual({
      pathname: '/',
      search: '?page=3&topic=Science',
    })
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
    sessionStorage.removeItem(LAST_FEED_SEARCH_KEY)
  })

  it('buildFeedReturnTo merges draft into page-only navigation state', () => {
    saveFeedPrefsDraft({
      topics: ['Health'],
      sourceIds: [2],
      sort: 'positivity',
      minPositivity: 0.6,
    })
    expect(buildFeedReturnTo('?page=2')).toEqual({
      pathname: '/',
      search: '?page=2&topic=Health&source=2&sort=positivity&minPositivity=0.6',
    })
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })

  it('buildFeedReturnTo restores session draft when state and last search are missing', () => {
    sessionStorage.removeItem(LAST_FEED_SEARCH_KEY)
    saveFeedPrefsDraft({
      topics: ['Science'],
      sourceIds: [2],
      sort: 'positivity',
      minPositivity: 0.6,
    })
    expect(buildFeedReturnTo()).toEqual({
      pathname: '/',
      search: '?topic=Science&source=2&sort=positivity&minPositivity=0.6&page=1',
    })
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })

  it('detects preference keys in the URL', () => {
    expect(hasPreferenceParamsInUrl(new URLSearchParams())).toBe(false)
    expect(hasPreferenceParamsInUrl(new URLSearchParams('page=2&settings=1'))).toBe(false)
    expect(hasPreferenceParamsInUrl(new URLSearchParams('topic=Health'))).toBe(true)
  })

  it('shouldHydrateFeedFromDraft when bare URL and session draft differ', () => {
    saveFeedPrefsDraft({
      topics: ['Health'],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    expect(shouldHydrateFeedFromDraft(new URLSearchParams())).toBe(true)
    expect(shouldHydrateFeedFromDraft(new URLSearchParams('topic=Health'))).toBe(false)
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })

  it('should not hydrate bare URL when draft only has default sort (e.g. after switching to date)', () => {
    saveFeedPrefsDraft({
      topics: [],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    expect(shouldHydrateFeedFromDraft(new URLSearchParams())).toBe(false)
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })

  it('omits sort from URL when sort is publication date', () => {
    const params = applyPreferencesToSearchParams(new URLSearchParams('sort=positivity'), {
      topics: [],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    expect(params.get('sort')).toBeNull()
  })

  it('mergeDraftIntoSearchParams preserves page and settings', () => {
    const draft = {
      topics: ['Science'],
      sourceIds: [],
      sort: 'date' as const,
      minPositivity: DEFAULT_MIN_POSITIVITY,
    }
    const merged = mergeDraftIntoSearchParams(
      new URLSearchParams('page=3&settings=1'),
      draft,
    )
    expect(merged.get('page')).toBe('3')
    expect(merged.get('settings')).toBe('1')
    expect(merged.getAll('topic')).toEqual(['Science'])
  })

  it('clearLocalFeedPreferences removes draft and last feed search', () => {
    saveFeedPrefsDraft({
      topics: ['Health'],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    saveLastFeedSearch('?topic=Health&page=2')
    saveLastSavedPreferenceParams(1, 'topic=Health')
    clearLocalFeedPreferences()
    expect(sessionStorage.getItem(FEED_PREFS_DRAFT_KEY)).toBeNull()
    expect(sessionStorage.getItem(LAST_FEED_SEARCH_KEY)).toBeNull()
    expect(sessionStorage.getItem(FEED_PREFS_LAST_SAVED_KEY)).toBeNull()
    expect(buildFeedReturnTo()).toEqual({ pathname: '/' })
  })

  it('stores and loads last saved preference params per user', () => {
    saveLastSavedPreferenceParams(1, 'topic=Health')
    expect(loadLastSavedPreferenceParams(1)).toBe('topic=Health')
    expect(loadLastSavedPreferenceParams(2)).toBeNull()
    sessionStorage.removeItem(FEED_PREFS_LAST_SAVED_KEY)
  })

  it('serializePreferenceSnapshot matches URL serialization', () => {
    const params = new URLSearchParams('topic=Health&source=2&sort=positivity&minPositivity=0.6')
    const snapshot = preferencesFromSearchParams(params)
    expect(serializePreferenceSnapshot(snapshot)).toBe(serializePreferenceParams(params))
  })

  it('hasNonDefaultPreferences is false for empty snapshot', () => {
    expect(
      hasNonDefaultPreferences({
        topics: [],
        sourceIds: [],
        sort: 'date',
        minPositivity: DEFAULT_MIN_POSITIVITY,
      }),
    ).toBe(false)
  })
})
