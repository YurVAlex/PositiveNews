/** Full article view: loads by route id, renders HTML body, and hosts comments. */
import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation, useParams } from 'react-router-dom'
import { fetchArticleDetail } from '../api/articles-api'
import type { ArticleDetailResponse } from '../api/types'
import { useAuth } from '../auth/AuthProvider'
import { ArticleCommentsSection } from '../components/comments/ArticleCommentsSection'
import { buildFeedReturnTo } from '../utils/feed-preferences-url'

type ArticleDetailLocationState = {
  feedSearch?: string
}

function formatDetailDate(iso: string) {
  const d = new Date(iso)
  return d.toLocaleString(undefined, { month: 'long', day: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

/** Link that returns to the feed with the same filters the user had when they opened the article. */
function BackToFeedLink({ className }: { className?: string }) {
  const location = useLocation()
  const feedReturnTo = useMemo(() => {
    const state = location.state as ArticleDetailLocationState | null
    return buildFeedReturnTo(state?.feedSearch)
  }, [location.state])

  return (
    <Link
      to={feedReturnTo}
      className={['text-decoration-none d-inline-block', className].filter(Boolean).join(' ')}
    >
      &larr; Back to Feed
    </Link>
  )
}

export function ArticleDetailPage() {
  const { token } = useAuth()
  const { id } = useParams()
  const numericId = Number(id)

  // undefined = loading, null = not found or invalid, object = loaded article.
  const [article, setArticle] = useState<ArticleDetailResponse | null | undefined>(undefined)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!Number.isFinite(numericId)) {
      setArticle(null)
      return
    }

    // Token is optional: guests can read articles; signed-in users may get extra fields from the API.
    let cancelled = false
    setError(null)

    ;(async () => {
      try {
        const res = await fetchArticleDetail(numericId, token)
        if (!cancelled) {
          setArticle(res)
        }
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof Error ? e.message : 'Failed to load article')
          setArticle(null)
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [numericId, token])

  useEffect(() => {
    if (article?.title) {
      document.title = `${article.title} - PositiveNews.Web`
    }
  }, [article?.title])

  if (!Number.isFinite(numericId)) {
    return (
      <main className="mt-4 mb-5">
        <div className="alert alert-warning">Invalid article id.</div>
      </main>
    )
  }

  if (error) {
    return (
      <main className="mt-4 mb-5">
        <div className="alert alert-danger">{error}</div>
      </main>
    )
  }

  if (article === undefined) {
    return (
      <main className="mt-4 mb-5">
        <div className="alert alert-secondary mb-0">Loading…</div>
      </main>
    )
  }

  if (article === null) {
    return (
      <main className="mt-4 mb-5">
        <div className="alert alert-warning">Article not found.</div>
      </main>
    )
  }

  return (
    <main className="mt-4 mb-5">
      <div className="row justify-content-center">
        <div className="col-md-12">
          <div className="d-flex justify-content-between">
            <BackToFeedLink className="mb-4" />
            <div>
              {article.sourceLogoUrl ? (
                <img
                  src={article.sourceLogoUrl}
                  alt={article.sourceName}
                  className="border-light me-2"
                  style={{ width: 32, height: 32, objectFit: 'cover' }}
                />
              ) : null}
              <span className="fw-bold me-2">{article.sourceName}</span>
            </div>
          </div>

          <h1 className="fw-bold mb-3">{article.title}</h1>

          <div className="d-flex align-items-center text-muted mb-4 border-bottom pb-3">
            <span>•</span>
            <span className="mx-2">{article.author?.trim().length ? article.author : 'Unknown Author'}</span>
            <span>•</span>
            <span className="ms-2">{formatDetailDate(article.publishedAt)}</span>
          </div>

          {article.contentHtml?.trim() ? (
            <div
              className="article-content fs-5"
              style={{ lineHeight: 1.8 }}
              dangerouslySetInnerHTML={{ __html: article.contentHtml }}
            />
          ) : (
            <div className="alert alert-warning">Full content is not available for this article yet.</div>
          )}

          <ArticleCommentsSection articleId={numericId} />

          <div className="mt-4 pt-3 border-top">
            <BackToFeedLink />
          </div>
        </div>
      </div>
    </main>
  )
}
