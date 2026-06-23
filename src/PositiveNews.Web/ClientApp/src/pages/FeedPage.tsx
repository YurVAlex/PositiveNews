/** Home feed: article list driven by URL query params (topics, sources, sort, page). */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { fetchArticleFeed, type FeedSortParam } from '../api/articles-api'
import type { ArticleFeedResponse } from '../api/types'
import { ArticleCard } from '../components/ArticleCard'
import { FeedActiveSources } from '../components/FeedActiveSources'
import { FeedActiveTopics } from '../components/FeedActiveTopics'
import { FeedPagination } from '../components/FeedPagination'
import { FeedSettingsPanel } from '../components/FeedSettingsPanel'
import { buildPreferenceSortHint, FeedSortSelect, feedSortModeLabel } from '../components/FeedSortSelect'
import { useAuth } from '../auth/AuthProvider'
import { usePersistFeedPreferences } from '../hooks/usePersistFeedPreferences'
import {
  applyPreferencesToSearchParams,
  buildDefaultFeedSearchParams,
  hasPreferenceParamsInUrl,
  isSettingsOpen,
  loadFeedPrefsDraft,
  mergeDraftIntoSearchParams,
  parseMinPositivity,
  parsePage,
  parseSort,
  preferencesFromSearchParams,
  saveFeedPrefsDraft,
  saveLastFeedSearch,
  serializePreferenceParams,
  serializePreferenceSnapshot,
  shouldHydrateFeedFromDraft,
  sourceIdsFromSearchParams,
  topicsFromSearchParams,
} from '../utils/feed-preferences-url'

/** Picks a heading that reflects active topic/source filters, or "Latest News" when none. */
function feedTitle(topics: string[], sourceCount: number, singleSourceName: string | null): string {
  const hasTopics = topics.length > 0
  const hasSources = sourceCount > 0

  if (hasTopics && hasSources) {
    return 'Your preferences'
  }
  if (hasSources) {
    return sourceCount === 1 && singleSourceName ? `Source: ${singleSourceName}` : 'Your preferences'
  }
  if (hasTopics) {
    return topics.length === 1 ? `Topic: ${topics[0]}` : 'Your preferences'
  }
  return 'Latest News'
}

export function FeedPage() {
  const { token, isLoading, isAuthenticated, user, pendingServerPreferences, clearPendingServerPreferences } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = useMemo(() => parsePage(searchParams.get('page')), [searchParams])
  const topics = useMemo(() => topicsFromSearchParams(searchParams), [searchParams])
  const sourceIds = useMemo(() => sourceIdsFromSearchParams(searchParams), [searchParams])
  const sortMode = useMemo(() => parseSort(searchParams.get('sort')), [searchParams])
  const minPositivity = useMemo(() => parseMinPositivity(searchParams.get('minPositivity')), [searchParams])
  const settingsOpen = useMemo(() => isSettingsOpen(searchParams), [searchParams])
  const hasPreferences = topics.length > 0 || sourceIds.length > 0
  // Serialized query string passed to article links so "Back to Feed" restores this view.
  const feedReturnSearch = useMemo(() => {
    const params = new URLSearchParams(searchParams)
    params.set('page', String(page))
    const qs = params.toString()
    return qs ? `?${qs}` : ''
  }, [searchParams, page])

  // Remember last feed URL for navigation from other routes (e.g. layout brand link).
  useEffect(() => {
    saveLastFeedSearch(feedReturnSearch)
  }, [feedReturnSearch])

  const [data, setData] = useState<ArticleFeedResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [saveError, setSaveError] = useState<string | null>(null)
  const didHydrateFromDraft = useRef(false)
  const wasAuthenticatedRef = useRef(isAuthenticated)

  // Every URL change also updates the session draft so prefs survive refresh and cross-route nav.
  const commitSearchParams = useCallback(
    (next: URLSearchParams, options?: { replace?: boolean }) => {
      saveFeedPrefsDraft(preferencesFromSearchParams(next))
      setSearchParams(next, options)
    },
    [setSearchParams],
  )

  // Logout clears personalized filters from the URL; guests use the default feed.
  useEffect(() => {
    const wasAuthenticated = wasAuthenticatedRef.current
    wasAuthenticatedRef.current = isAuthenticated
    if (wasAuthenticated && !isAuthenticated && hasPreferenceParamsInUrl(searchParams)) {
      setSearchParams(buildDefaultFeedSearchParams(), { replace: true })
    }
  }, [isAuthenticated, searchParams, setSearchParams])

  // After login/register, apply server prefs to the URL; clear pending only once the URL matches.
  useEffect(() => {
    if (!pendingServerPreferences) return

    const expectedSerialized = serializePreferenceSnapshot(pendingServerPreferences)
    const currentSerialized = serializePreferenceParams(searchParams)

    if (currentSerialized === expectedSerialized) {
      didHydrateFromDraft.current = true
      clearPendingServerPreferences()
      return
    }

    const next = applyPreferencesToSearchParams(new URLSearchParams(), pendingServerPreferences, {
      includeSettings: true,
      settingsOpen: isSettingsOpen(searchParams),
      page: 1,
    })
    saveFeedPrefsDraft(preferencesFromSearchParams(next))
    setSearchParams(next, { replace: true })
  }, [pendingServerPreferences, searchParams, clearPendingServerPreferences, setSearchParams])

  // On first visit to bare "/", restore filters from session draft if the URL has no prefs yet.
  useEffect(() => {
    if (pendingServerPreferences) return
    if (didHydrateFromDraft.current) return
    didHydrateFromDraft.current = true
    if (!shouldHydrateFeedFromDraft(searchParams)) return
    const draft = loadFeedPrefsDraft()
    if (!draft) return
    commitSearchParams(mergeDraftIntoSearchParams(searchParams, draft), { replace: true })
    // eslint-disable-next-line react-hooks/exhaustive-deps -- hydrate bare / from session draft once on first feed mount
  }, [pendingServerPreferences, searchParams, commitSearchParams])

  // Keep session draft in sync when the URL changes (e.g. browser back/forward).
  useEffect(() => {
    const snapshot = preferencesFromSearchParams(searchParams)
    saveFeedPrefsDraft(snapshot)
  }, [searchParams])

  // Fetch and auto-save only after the URL reflects pending server prefs (avoids stale-query requests).
  const prefsReady = useMemo(() => {
    if (!pendingServerPreferences) {
      return true
    }
    return (
      serializePreferenceParams(searchParams) ===
      serializePreferenceSnapshot(pendingServerPreferences)
    )
  }, [pendingServerPreferences, searchParams])

  // Debounced sync of URL preferences to the server for signed-in users.
  usePersistFeedPreferences(searchParams, token, isAuthenticated, user?.id ?? null, setSaveError, prefsReady)

  // Refetch articles whenever filters or auth token change; ignore stale responses.
  useEffect(() => {
    if (isLoading || !prefsReady) return

    let cancelled = false
    setError(null)

    ;(async () => {
      try {
        const res = await fetchArticleFeed(page, topics, sourceIds, sortMode, token, minPositivity)
        if (!cancelled) {
          setData(res)
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : 'Failed to load feed')
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [page, topics, sourceIds, sortMode, minPositivity, token, isLoading, prefsReady])

  // Merge a partial preference change into the URL and reset to page 1.
  const updatePreferences = useCallback(
    (patch: Partial<{ topics: string[]; sourceIds: number[]; sort: FeedSortParam; minPositivity: number }>) => {
      const current = preferencesFromSearchParams(searchParams)
      const snapshot = {
        topics: patch.topics ?? current.topics,
        sourceIds: patch.sourceIds ?? current.sourceIds,
        sort: patch.sort ?? current.sort,
        minPositivity: patch.minPositivity ?? current.minPositivity,
      }
      const next = applyPreferencesToSearchParams(new URLSearchParams(searchParams), snapshot, {
        includeSettings: true,
        settingsOpen,
        page: 1,
      })
      commitSearchParams(next)
    },
    [searchParams, commitSearchParams, settingsOpen],
  )

  // Build a feed URL that adds/removes one topic—used by topic chips on cards without inline navigation logic.
  const buildTopicToggleUrl = useCallback(
    (topicName: string) => {
      const trimmed = topicName.trim()
      if (!trimmed.length) return `/?${searchParams.toString()}`

      const current = preferencesFromSearchParams(searchParams)
      const lower = trimmed.toLowerCase()
      const exists = current.topics.some((t) => t.toLowerCase() === lower)
      const nextTopics = exists
        ? current.topics.filter((t) => t.toLowerCase() !== lower)
        : [...current.topics, trimmed]

      const next = applyPreferencesToSearchParams(new URLSearchParams(searchParams), {
        ...current,
        topics: nextTopics,
      }, { includeSettings: true, settingsOpen, page: 1 })
      return `/?${next.toString()}`
    },
    [searchParams, settingsOpen],
  )

  // Same pattern as buildTopicToggleUrl, but for source filters.
  const buildSourceToggleUrl = useCallback(
    (sourceId: number) => {
      if (!Number.isInteger(sourceId) || sourceId < 1) return `/?${searchParams.toString()}`

      const current = preferencesFromSearchParams(searchParams)
      const exists = current.sourceIds.includes(sourceId)
      const nextSourceIds = exists
        ? current.sourceIds.filter((id) => id !== sourceId)
        : [...current.sourceIds, sourceId]

      const next = applyPreferencesToSearchParams(new URLSearchParams(searchParams), {
        ...current,
        sourceIds: nextSourceIds,
      }, { includeSettings: true, settingsOpen, page: 1 })
      return `/?${next.toString()}`
    },
    [searchParams, settingsOpen],
  )

  const singleSourceName =
    data?.selectedSources.length === 1 ? data.selectedSources[0].name : null

  const title = feedTitle(topics, data?.selectedSources.length ?? sourceIds.length, singleSourceName)

  const documentTitle =
    topics.length === 0 && sourceIds.length === 0
      ? 'Positive News Feed'
      : `${title} - Articles`

  useEffect(() => {
    document.title = `${documentTitle} - PositiveNews.Web`
  }, [documentTitle])

  // Scroll to top only after the fetched page matches the requested page (avoids jumping during load).
  useEffect(() => {
    if (data?.currentPage === page) {
      window.scrollTo(0, 0)
    }
  }, [data, page])

  const setPage = useCallback(
    (nextPage: number) => {
      const next = new URLSearchParams(searchParams)
      next.set('page', String(nextPage))
      commitSearchParams(next)
    },
    [searchParams, commitSearchParams],
  )

  const setSortMode = useCallback(
    (next: FeedSortParam) => {
      updatePreferences({ sort: next })
    },
    [updatePreferences],
  )

  const closeSettings = useCallback(() => {
    const params = new URLSearchParams(searchParams)
    params.delete('settings')
    commitSearchParams(params)
  }, [searchParams, commitSearchParams])

  const sortLabel = feedSortModeLabel(sortMode)
  const preferenceSortHint = buildPreferenceSortHint(sortMode, sortLabel)

  if (error) {
    return (
      <main role="main" className="pb-3 mt-4">
        <div className="alert alert-danger">{error}</div>
      </main>
    )
  }

  if (!data) {
    return (
      <main role="main" className="pb-3 mt-4">
        <div className="alert alert-secondary mb-0">Loading…</div>
      </main>
    )
  }

  return (
    <main role="main" className="pb-2 mt-1">
      <div className="row justify-content-center">
        <div className="col-md-12">
          {settingsOpen ? (
            <FeedSettingsPanel
              selectedTopics={topics}
              selectedSourceIds={sourceIds}
              minPositivity={minPositivity}
              onTopicsChange={(nextTopics) => updatePreferences({ topics: nextTopics })}
              onSourcesChange={(nextSourceIds) => updatePreferences({ sourceIds: nextSourceIds })}
              onMinPositivityCommit={(value) => updatePreferences({ minPositivity: value })}
              onClose={closeSettings}
              token={token}
            />
          ) : null}

          {saveError ? (
            <div className="alert alert-warning py-2 mb-2" role="alert">
              {saveError}
            </div>
          ) : null}

          <div className="d-flex justify-content-between align-items-center mb-2 flex-wrap gap-3">
            <h3 className="mb-0">{title}</h3>
            <FeedPagination
              currentPage={data.currentPage}
              totalPages={data.totalPages}
              onPageChange={setPage}
              className="d-flex align-items-center"
            />
            <FeedSortSelect
              sortMode={sortMode}
              hasPreferences={hasPreferences}
              onSortChange={setSortMode}
              className="ms-md-auto"
            />
          </div>

          {!settingsOpen ? (
            <>
              <FeedActiveTopics
                topics={topics}
                buildTopicToggleUrl={buildTopicToggleUrl}
                hint={preferenceSortHint}
              />

              <FeedActiveSources
                sources={data.selectedSources}
                buildSourceToggleUrl={buildSourceToggleUrl}
                hint={topics.length === 0 ? preferenceSortHint : null}
              />
            </>
          ) : null}

          {data.articles.map((a, i) => (
            <ArticleCard
              key={a.id}
              article={a}
              index={i}
              selectedTopics={topics}
              buildTopicToggleUrl={buildTopicToggleUrl}
              selectedSourceIds={sourceIds}
              buildSourceToggleUrl={buildSourceToggleUrl}
              feedReturnSearch={feedReturnSearch}
            />
          ))}

          <FeedPagination
            currentPage={data.currentPage}
            totalPages={data.totalPages}
            onPageChange={setPage}
            className="d-flex justify-content-center"
            listClassName="justify-content-center"
          />
        </div>
      </div>
    </main>
  )
}
