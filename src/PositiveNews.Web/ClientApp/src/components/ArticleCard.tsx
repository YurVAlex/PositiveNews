import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { ArticlePreviewResponse } from '../api/types'
import { ArticleImage } from './ArticleImage'
import { ArticleTopicLinks } from './ArticleTopicLinks'

function formatPublishedAt(iso: string) {
  const d = new Date(iso)
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
}

type ArticleCardProps = {
  article: ArticlePreviewResponse
  index: number
  selectedTopic: string | null | undefined
}

export function ArticleCard({ article, index, selectedTopic }: ArticleCardProps) {
  const [summaryOpen, setSummaryOpen] = useState(false)

  return (
    <div className="card mb-4 shadow-sm">
      <div className="card-header bg-white d-flex align-items-center border-0 pb-0">
        {article.sourceLogoUrl ? (
          <img
            src={article.sourceLogoUrl}
            alt={article.sourceName}
            className="me-2"
            style={{ width: 32, height: 32, objectFit: 'cover' }}
          />
        ) : null}
        <span className="fw-bold text-muted fs-5">{article.sourceName}</span>
      </div>

      <div className="card-body">
        <h4 className="card-title fw-bold">{article.title}</h4>

        <h6 className="card-subtitle mb-3 text-muted">
          {(article.author?.trim().length ? article.author : 'Unknown Author') + ' • ' + formatPublishedAt(article.publishedAt)}
        </h6>

        <ArticleImage imageTag={article.imageTag} index={index} />

        <ArticleTopicLinks topics={article.topics} selectedTopic={selectedTopic} />

        <div className="d-flex gap-2 mt-3">
          <button type="button" className="btn btn-outline-secondary" onClick={() => setSummaryOpen((o) => !o)}>
            {summaryOpen ? 'Hide summary' : 'Show summary'}
          </button>

          <Link to={`/articles/${article.id}`} className="btn btn-primary">
            Read article
          </Link>
        </div>

        <div className={`mt-3 p-3 bg-light rounded ${summaryOpen ? '' : 'd-none'}`}>
          <strong>Summary:</strong>
          <p className="mb-0">{article.summaryShort}</p>
        </div>
      </div>
    </div>
  )
}
