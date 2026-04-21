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
    const hasPreviewImage = Boolean(article.imageTag?.trim())

    return (
        <div className="card mb-4 shadow-sm overflow-hidden">
            <div
                className={`article-card-layout ${!hasPreviewImage ? 'article-card-layout--no-image' : ''}`}
            >
                <div className="article-card-hdr card-header bg-white flex-column align-items-start border-0 pb-0">
                    {article.sourceLogoUrl ? (
                        <img
                            src={article.sourceLogoUrl}
                            alt={article.sourceName}
                            className="me-2"
                            style={{ width: 32, height: 32, objectFit: 'cover' }}
                        />
                    ) : null}
                    <span className="fw-bold text-muted fs-5">{article.sourceName}</span>
                    <h6 className="card-subtitle pt-2 mb-0 text-muted">
                        {(article.author?.trim().length ? article.author : 'Unknown Author') + ' • ' + formatPublishedAt(article.publishedAt)}
                        </h6>
                </div>
                <div className="article-card-title card-body">
                    <h4 className="card-title fw-bold">{article.title}</h4>
                </div>

                {hasPreviewImage ? (
                    <div className="article-card-image">
                        <ArticleImage imageTag={article.imageTag} index={index} />
                    </div>
                ) : null}

                <div className="article-card-body card-body pt-0 border-0">
                    <ArticleTopicLinks topics={article.topics} selectedTopic={selectedTopic} />

                    <div className="d-flex flex-wrap gap-2 mt-2">
                        <button type="button" className="btn btn-outline-secondary" onClick={() => setSummaryOpen((o) => !o)}>
                            {summaryOpen ? 'Hide summary' : 'Show summary'}
                        </button>

                        <Link to={`/articles/${article.id}`} className="btn btn-primary">
                            Read article
                        </Link>
                    </div>

                    <div className={`mt-3 bg-light rounded ${summaryOpen ? '' : 'd-none'}`}>
                        <strong>Summary:</strong>
                        <p className="mb-0">{article.summaryShort}</p>
                    </div>
                </div>
            </div>
        </div>
    )
}
