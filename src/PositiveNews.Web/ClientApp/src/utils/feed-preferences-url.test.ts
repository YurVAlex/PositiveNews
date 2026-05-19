import { describe, expect, it } from 'vitest'
import {
  applyPreferencesToSearchParams,
  buildFeedReturnPath,
  DEFAULT_MIN_POSITIVITY,
  FEED_PREFS_DRAFT_KEY,
  parseMinPositivity,
  parseSort,
  preferencesFromSearchParams,
  saveFeedPrefsDraft,
  serializePreferenceParams,
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

  it('buildFeedReturnPath prefers navigation state over session draft', () => {
    saveFeedPrefsDraft({
      topics: ['Draft'],
      sourceIds: [],
      sort: 'date',
      minPositivity: DEFAULT_MIN_POSITIVITY,
    })
    expect(buildFeedReturnPath('?topic=Health')).toBe('/?topic=Health')
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })

  it('buildFeedReturnPath restores session draft when state is missing', () => {
    saveFeedPrefsDraft({
      topics: ['Science'],
      sourceIds: [2],
      sort: 'positivity',
      minPositivity: 0.6,
    })
    expect(buildFeedReturnPath()).toBe('/?topic=Science&source=2&sort=positivity&minPositivity=0.6&page=1')
    sessionStorage.removeItem(FEED_PREFS_DRAFT_KEY)
  })
})
