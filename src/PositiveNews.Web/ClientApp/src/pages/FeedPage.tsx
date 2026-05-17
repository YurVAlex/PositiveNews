import { useCallback, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { fetchArticleFeed, type FeedSortParam } from '../api/articles-api'
import type { ArticleFeedResponse } from '../api/types'
import { ArticleCard } from '../components/ArticleCard'
import { FeedActiveSources } from '../components/FeedActiveSources'
import { FeedActiveTopics } from '../components/FeedActiveTopics'
import { FeedPagination } from '../components/FeedPagination'
import { buildPreferenceSortHint, FeedSortSelect, feedSortModeLabel } from '../components/FeedSortSelect'
import { useAuth } from '../auth/AuthProvider'

function parsePage(raw: string | null) {
  const n = Number(raw ?? '1')
  if (!Number.isFinite(n) || n < 1) {
    return 1
  }
  return Math.floor(n)
}

/** Distinct topics from query string, preserving first-seen casing. */
function topicsFromSearchParams(searchParams: URLSearchParams): string[] {
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

/** Distinct source ids from query string, preserving first-seen order. */
function sourceIdsFromSearchParams(searchParams: URLSearchParams): number[] {
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

function parseSort(raw: string | null): FeedSortParam {
  const value = raw?.toLowerCase()
  if (value === 'positivity') return 'positivity'
  if (value === 'preferences') return 'preferences'
  return 'date'
}

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
  const { token } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = useMemo(() => parsePage(searchParams.get('page')), [searchParams])
  const topics = useMemo(() => topicsFromSearchParams(searchParams), [searchParams])
  const sourceIds = useMemo(() => sourceIdsFromSearchParams(searchParams), [searchParams])
  const sortMode = useMemo(() => parseSort(searchParams.get('sort')), [searchParams])
  const hasPreferences = topics.length > 0 || sourceIds.length > 0

  const [data, setData] = useState<ArticleFeedResponse | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setError(null)

    ;(async () => {
      try {
        const res = await fetchArticleFeed(page, topics, sourceIds, sortMode, token)
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
  }, [page, topics, sourceIds, sortMode, token])

  const buildTopicToggleUrl = useCallback(
    (topicName: string) => {
      const trimmed = topicName.trim()
      if (!trimmed.length) return `/?${searchParams.toString()}`

      const next = new URLSearchParams(searchParams)
      const lower = trimmed.toLowerCase()
      const exists = topics.some((t) => t.toLowerCase() === lower)
      next.delete('topic')
      next.set('page', '1')
      if (exists) {
        topics.filter((t) => t.toLowerCase() !== lower).forEach((t) => next.append('topic', t))
      } else {
        topics.forEach((t) => next.append('topic', t))
        next.append('topic', trimmed)
      }
      return `/?${next.toString()}`
    },
    [searchParams, topics],
  )

  const buildSourceToggleUrl = useCallback(
    (sourceId: number) => {
      if (!Number.isInteger(sourceId) || sourceId < 1) return `/?${searchParams.toString()}`

      const next = new URLSearchParams(searchParams)
      const exists = sourceIds.includes(sourceId)
      next.delete('source')
      next.set('page', '1')
      if (exists) {
        sourceIds.filter((id) => id !== sourceId).forEach((id) => next.append('source', String(id)))
      } else {
        sourceIds.forEach((id) => next.append('source', String(id)))
        next.append('source', String(sourceId))
      }
      return `/?${next.toString()}`
    },
    [searchParams, sourceIds],
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
      setSearchParams(next)
    },
    [searchParams, setSearchParams],
  )

  const setSortMode = useCallback(
    (next: FeedSortParam) => {
      const params = new URLSearchParams(searchParams)
      if (next === 'date') {
        params.delete('sort')
      } else {
        params.set('sort', next)
      }
      params.set('page', '1')
      setSearchParams(params)
    },
    [searchParams, setSearchParams],
  )

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

          {data.articles.map((a, i) => (
            <ArticleCard
              key={a.id}
              article={a}
              index={i}
              selectedTopics={topics}
              buildTopicToggleUrl={buildTopicToggleUrl}
              selectedSourceIds={sourceIds}
              buildSourceToggleUrl={buildSourceToggleUrl}
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
