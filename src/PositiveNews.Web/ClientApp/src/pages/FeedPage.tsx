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
  shouldHydrateFeedFromDraft,
  sourceIdsFromSearchParams,
  topicsFromSearchParams,
} from '../utils/feed-preferences-url'

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
  const { token, isAuthenticated, user, pendingServerPreferences, clearPendingServerPreferences } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = useMemo(() => parsePage(searchParams.get('page')), [searchParams])
  const topics = useMemo(() => topicsFromSearchParams(searchParams), [searchParams])
  const sourceIds = useMemo(() => sourceIdsFromSearchParams(searchParams), [searchParams])
  const sortMode = useMemo(() => parseSort(searchParams.get('sort')), [searchParams])
  const minPositivity = useMemo(() => parseMinPositivity(searchParams.get('minPositivity')), [searchParams])
  const settingsOpen = useMemo(() => isSettingsOpen(searchParams), [searchParams])
  const hasPreferences = topics.length > 0 || sourceIds.length > 0
  const feedReturnSearch = useMemo(() => {
    const params = new URLSearchParams(searchParams)
    params.set('page', String(page))
    const qs = params.toString()
    return qs ? `?${qs}` : ''
  }, [searchParams, page])

  useEffect(() => {
    saveLastFeedSearch(feedReturnSearch)
  }, [feedReturnSearch])

  const [data, setData] = useState<ArticleFeedResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [saveError, setSaveError] = useState<string | null>(null)
  const didHydrateFromDraft = useRef(false)
  const wasAuthenticatedRef = useRef(isAuthenticated)

  const commitSearchParams = useCallback(
    (next: URLSearchParams, options?: { replace?: boolean }) => {
      saveFeedPrefsDraft(preferencesFromSearchParams(next))
      setSearchParams(next, options)
    },
    [setSearchParams],
  )

  useEffect(() => {
    const wasAuthenticated = wasAuthenticatedRef.current
    wasAuthenticatedRef.current = isAuthenticated
    if (wasAuthenticated && !isAuthenticated && hasPreferenceParamsInUrl(searchParams)) {
      setSearchParams(buildDefaultFeedSearchParams(), { replace: true })
    }
  }, [isAuthenticated, searchParams, setSearchParams])

  useEffect(() => {
    if (!pendingServerPreferences) return
    const next = applyPreferencesToSearchParams(new URLSearchParams(), pendingServerPreferences, {
      includeSettings: true,
      settingsOpen: isSettingsOpen(searchParams),
      page: 1,
    })
    saveFeedPrefsDraft(preferencesFromSearchParams(next))
    setSearchParams(next, { replace: true })
    clearPendingServerPreferences()
    // eslint-disable-next-line react-hooks/exhaustive-deps -- apply server snapshot once when auth provides it
  }, [pendingServerPreferences, clearPendingServerPreferences, setSearchParams])

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

  useEffect(() => {
    const snapshot = preferencesFromSearchParams(searchParams)
    saveFeedPrefsDraft(snapshot)
  }, [searchParams])

  usePersistFeedPreferences(searchParams, token, isAuthenticated, user?.id ?? null, setSaveError)

  useEffect(() => {
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
  }, [page, topics, sourceIds, sortMode, minPositivity, token])

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
