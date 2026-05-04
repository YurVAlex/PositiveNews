import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { ArticlePreviewResponse } from '../api/types'
import { ArticleImage } from './ArticleImage'
import { ArticleTopicLinks } from './ArticleTopicLinks'

function formatPublishedAt(iso: string) {
    const d = new Date(iso)
    return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
}

function formatPositivityScore(score: number | null): string | null {
    if (score == null || Number.isNaN(score)) return null
    const pct = Math.round(score * 100)
    return `${pct}% Positivity`
}

/** Red below 0.49; yellow from 0.49 through 0.51; green above 0.51 */
function positivityBadgeClassName(score: number): string {
    const base =
        'ms-auto text-nowrap small fw-semibold border rounded px-2 py-1'
    if (score < 0.49) return `${base} text-danger border-danger-subtle bg-danger-subtle`
    if (score <= 0.51) return `${base} text-dark border-warning-subtle bg-warning-subtle`
    return `${base} text-success border-success-subtle bg-success-subtle`
}

type ArticleCardProps = {
    article: ArticlePreviewResponse
    index: number
    selectedTopics: string[]
    buildTopicToggleUrl: (topic: string) => string
}

export function ArticleCard({ article, index, selectedTopics, buildTopicToggleUrl }: ArticleCardProps) {
    const [summaryOpen, setSummaryOpen] = useState(false)
    const hasPreviewImage = Boolean(article.imageTag?.trim())
    const positivityLabel = formatPositivityScore(article.positivityScore)
    const positivityBadgeClasses =
        article.positivityScore != null && !Number.isNaN(article.positivityScore)
            ? positivityBadgeClassName(article.positivityScore)
            : ''
    const originUrl = article.url?.trim() ?? ''

    return (
        <div className="card mb-4 shadow-sm overflow-hidden">
            <div
                className={`article-card-layout ${!hasPreviewImage ? 'article-card-layout--no-image' : ''}`}
            >
                <div className="article-card-hdr card-header bg-white border-0 pb-0 d-flex flex-column align-items-stretch">
                    <div className="d-flex w-100 align-items-center gap-2">
                        {article.sourceLogoUrl ? (
                            <img
                                src={article.sourceLogoUrl}
                                alt={article.sourceName}
                                className="flex-shrink-0"
                                style={{ width: 32, height: 32, objectFit: 'cover' }}
                            />
                        ) : null}
                        <span className="fw-bold text-muted fs-5 pt-2">{article.sourceName}</span>
                        {positivityLabel ? (
                            <span
                                className={positivityBadgeClasses}
                                title="Positivity score"
                            >
                                {positivityLabel}
                            </span>
                        ) : null}
                    </div>
                    
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
                    <ArticleTopicLinks
                        topics={article.topics}
                        selectedTopics={selectedTopics}
                        buildTopicToggleUrl={buildTopicToggleUrl}
                    />

                    <div className="d-flex flex-wrap gap-2 mt-2 align-items-center">
                        <button type="button" className="btn btn-outline-secondary" onClick={() => setSummaryOpen((o) => !o)}>
                            {summaryOpen ? 'Hide summary' : 'Show summary'}
                        </button>

                        <Link to={`/articles/${article.id}`} className="btn btn-primary">
                            Read article
                        </Link>

                        {originUrl ? (
                            <a href={originUrl} target="_blank" rel="noopener noreferrer" className="btn btn-outline-secondary">
                                Read origin
                            </a>
                        ) : null}
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
