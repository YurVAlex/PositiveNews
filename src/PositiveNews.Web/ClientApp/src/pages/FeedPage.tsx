import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { fetchArticleFeed } from '../api/articles-api'
import type { ArticleFeedResponse } from '../api/types'
import { ArticleCard } from '../components/ArticleCard'

function parsePage(raw: string | null) {
  const n = Number(raw ?? '1')
  if (!Number.isFinite(n) || n < 1) {
    return 1
  }
  return Math.floor(n)
}

export function FeedPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const page = useMemo(() => parsePage(searchParams.get('page')), [searchParams])
  const topic = useMemo(() => {
    const t = searchParams.get('topic')
    return t && t.trim().length > 0 ? t.trim() : null
  }, [searchParams])

  const [data, setData] = useState<ArticleFeedResponse | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setError(null)

    ;(async () => {
      try {
        const res = await fetchArticleFeed(page, topic)
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
  }, [page, topic])

  const title =
    topic == null || topic.length === 0 ? 'Latest News' : `Topic: ${topic}`

  const documentTitle =
    topic == null || topic.length === 0 ? 'Positive News Feed' : `Articles - ${topic}`

  useEffect(() => {
    document.title = `${documentTitle} - PositiveNews.Web`
  }, [documentTitle])

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

  function setPage(nextPage: number) {
    const next = new URLSearchParams(searchParams)
    next.set('page', String(nextPage))
    if (topic) {
      next.set('topic', topic)
    } else {
      next.delete('topic')
    }
    setSearchParams(next)
  }

  return (
    <main role="main" className="pb-2 mt-1">
      <div className="row justify-content-center">
        <div className="col-md-12">
          <div className="d-flex justify-content-between align-items-center mb-1">
            <h3>{title}</h3>
            {topic ? (
              <Link to="/" className="btn btn-outline-secondary btn-sm">
                ✕ Clear Filter
              </Link>
            ) : null}
          </div>

          {topic ? (
            <div className="alert alert-info mb-4">
              Showing articles with topic <strong>{topic}</strong> first.
            </div>
          ) : null}

          {data.articles.map((a, i) => (
            <ArticleCard key={a.id} article={a} index={i} selectedTopic={data.selectedTopic ?? topic} />
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
