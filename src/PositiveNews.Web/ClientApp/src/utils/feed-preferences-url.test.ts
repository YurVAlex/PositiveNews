import { describe, expect, it } from 'vitest'
import {
  applyPreferencesToSearchParams,
  buildFeedReturnTo,
  DEFAULT_MIN_POSITIVITY,
  FEED_PREFS_DRAFT_KEY,
  LAST_FEED_SEARCH_KEY,
  hasNonDefaultPreferences,
  hasPreferenceParamsInUrl,
  mergeDraftIntoSearchParams,
  parseMinPositivity,
  parseSort,
  preferencesFromSearchParams,
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

  it('buildFeedReturnTo uses last feed search before preference draft', () => {
    saveFeedPrefsDraft({
      topics: ['Science'],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    saveLastFeedSearch('?page=3')
    expect(buildFeedReturnTo()).toEqual({ pathname: '/', search: '?page=3' })
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
    sessionStorage.removeItem(LAST_FEED_SEARCH_KEY)
  })

  it('buildFeedReturnTo restores session draft when state and last search are missing', () => {
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
