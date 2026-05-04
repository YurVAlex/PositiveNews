import { useCallback, useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { fetchArticleFeed, type FeedSortParam } from '../api/articles-api'
import type { ArticleFeedResponse } from '../api/types'
import { ArticleCard } from '../components/ArticleCard'

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

function parseSort(raw: string | null): FeedSortParam {
  return raw?.toLowerCase() === 'positivity' ? 'positivity' : 'date'
}

export function FeedPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = useMemo(() => parsePage(searchParams.get('page')), [searchParams])
  const topics = useMemo(() => topicsFromSearchParams(searchParams), [searchParams])
  const sortMode = useMemo(() => parseSort(searchParams.get('sort')), [searchParams])

  const [data, setData] = useState<ArticleFeedResponse | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setError(null)

    ;(async () => {
      try {
        const res = await fetchArticleFeed(page, topics, sortMode)
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
  }, [page, topics, sortMode])

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

  const title =
    topics.length === 0 ? 'Latest News' : topics.length === 1 ? `Topic: ${topics[0]}` : 'Preferred topics'

  const documentTitle =
    topics.length === 0
      ? 'Positive News Feed'
      : topics.length === 1
        ? `Articles - ${topics[0]}`
        : 'Articles - preferred topics'

  useEffect(() => {
    document.title = `${documentTitle} - PositiveNews.Web`
  }, [documentTitle])

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
        params.set('sort', 'positivity')
      }
      params.set('page', '1')
      setSearchParams(params)
    },
    [searchParams, setSearchParams],
  )

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
            <div className="d-flex align-items-center gap-2 ms-md-auto">
              <label htmlFor="feed-sort-select" className="small text-muted mb-0 text-nowrap">
                Sort by
              </label>
              <select
                id="feed-sort-select"
                className="form-select form-select-sm"
                style={{ width: 'auto', minWidth: '11rem' }}
                aria-label="Sort articles"
                value={sortMode}
                onChange={(e) =>
                  setSortMode(e.target.value === 'positivity' ? 'positivity' : 'date')
                }
              >
                <option value="date">Publication date</option>
                <option value="positivity">Positivity score</option>
              </select>
            </div>
          </div>

          {topics.length > 0 ? (
            <div className="alert alert-info mb-3">
              <div className="mb-2">
                Prefered topics will be shown first, sorted by:{' '}
                <strong>{sortMode === 'positivity' ? 'positivity score' : 'publication date'}</strong>.
              </div>
              <div className="d-flex flex-wrap align-items-center gap-2">
                <span className="small text-muted me-1">Active:</span>
                {topics.map((t) => (
                  <Link
                    key={t}
                    to={buildTopicToggleUrl(t)}
                    className="btn btn-sm btn-primary"
                    title={`Remove “${t}” from preferred topics`}
                  >
                    {t}
                    <span className="ms-1 opacity-75" aria-hidden="true">
                      ×
                    </span>
                  </Link>
                ))}
              </div>
            </div>
          ) : null}

          {data.articles.map((a, i) => (
            <ArticleCard
              key={a.id}
              article={a}
              index={i}
              selectedTopics={topics}
              buildTopicToggleUrl={buildTopicToggleUrl}
            />
          ))}

          {data.totalPages > 1 ? (
            <nav>
              <ul className="pagination justify-content-center">
                <li className={`page-item ${data.currentPage === 1 ? 'disabled' : ''}`}>
                  <button
                    type="button"
                    className="page-link"
                    disabled={data.currentPage === 1}
                    onClick={() => setPage(data.currentPage - 1)}
                  >
                    Previous
                  </button>
                </li>

                <li className="page-item disabled">
                  <span className="page-link">
                    Page {data.currentPage} of {data.totalPages}
                  </span>
                </li>

                <li className={`page-item ${data.currentPage === data.totalPages ? 'disabled' : ''}`}>
                  <button
                    type="button"
                    className="page-link"
                    disabled={data.currentPage === data.totalPages}
                    onClick={() => setPage(data.currentPage + 1)}
                  >
                    Next
                  </button>
                </li>
              </ul>
            </nav>
          ) : null}
        </div>
      </div>
    </main>
  )
}
