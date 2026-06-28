/**
 * Admin panel: search articles, review details, and apply moderation changes.
 */
import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'

import {
  fetchAdminArticles,
  fetchAdminArticleDetail,
  moderateArticle,
  type AdminArticleDetail,
  type AdminArticleItem,
  type ArticleModerationRequest,
} from '../../api/admin-articles-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'
import { formatModeratedBy } from '../../utils/format-moderated-by'

export function ArticlesModeration() {
  const { token } = useAuth()
  const [articles, setArticles] = useState<AdminArticleItem[]>([])
  const [selectedArticleId, setSelectedArticleId] = useState<number | null>(null)
  const [selectedArticleDetail, setSelectedArticleDetail] = useState<AdminArticleDetail | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [searchPerformed, setSearchPerformed] = useState(false)
  const [loading, setLoading] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)
  const [submitLoading, setSubmitLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)
  const [formState, setFormState] = useState<ArticleModerationRequest>({
    isActive: false,
    title: '',
    imageTag: '',
    positivityScore: null,
    summaryShort: '',
    contentRaw: '',
    reason: '',
    note: '',
  })

  // Load full article detail and reset the moderation form when selection changes.
  useEffect(() => {
    if (selectedArticleId === null || !token) {
      setSelectedArticleDetail(null)
      return
    }

    setDetailLoading(true)
    setError(null)
    void fetchAdminArticleDetail(token, selectedArticleId)
      .then((detail) => {
        setSelectedArticleDetail(detail)
        setFormState({
          isActive: detail.isActive,
          title: detail.title,
          imageTag: detail.imageTag ?? '',
          positivityScore: detail.positivityScore ?? null,
          summaryShort: detail.summaryShort ?? '',
          contentRaw: detail.contentRaw ?? '',
          reason: '',
          note: '',
        })
        setSubmitMessage(null)
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load article details'))
      .finally(() => setDetailLoading(false))
  }, [selectedArticleId, token])

  /** Searches articles by title or id and populates the results table. */
  const handleSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token) return

    setError(null)
    setLoading(true)
    setSubmitMessage(null)

    try {
      const items = await fetchAdminArticles(token, searchTerm.trim())
      setArticles(items)
      setSelectedArticleId(null)
      setSelectedArticleDetail(null)
      setSearchPerformed(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search request failed')
    } finally {
      setLoading(false)
    }
  }

  const handleSelectArticle = (id: number) => {
    setSelectedArticleId(id)
    setSubmitMessage(null)
    setError(null)
  }

  /** Resets search, selection, and form state to initial values. */
  const handleClearSelection = () => {
    setError(null)
    setSubmitMessage(null)
    setSearchTerm('')
    setSearchPerformed(false)
    setSelectedArticleId(null)
    setSelectedArticleDetail(null)
    setArticles([])
    setFormState({
      isActive: false,
      title: '',
      imageTag: '',
      positivityScore: null,
      summaryShort: '',
      contentRaw: '',
      reason: '',
      note: '',
    })
  }

  /** Deselects the current article without clearing search results. */
  const handleCancel = () => {
    setSelectedArticleId(null)
    setSelectedArticleDetail(null)
    setSubmitMessage(null)
    setError(null)
    setFormState({
      isActive: false,
      title: '',
      imageTag: '',
      positivityScore: null,
      summaryShort: '',
      contentRaw: '',
      reason: '',
      note: '',
    })
  }

  /** Syncs controlled form fields from input change events. */
  const handleFormChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement
    const value = target.type === 'checkbox'
      ? ('checked' in target ? target.checked : false)
      : target.type === 'number'
      ? target.value === ''
        ? null
        : Number(target.value)
      : target.value

    setFormState((current) => ({
      ...current,
      [target.name]: value,
    }))
  }

  /** Persists moderation changes and refreshes list and detail from the server. */
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token || selectedArticleId === null) return

    setSubmitLoading(true)
    setError(null)
    setSubmitMessage(null)

    try {
      await moderateArticle(token, selectedArticleId, formState)
      setSubmitMessage('Article moderation saved successfully.')
      void fetchAdminArticles(token, searchTerm.trim()).then((items) => setArticles(items))
      void fetchAdminArticleDetail(token, selectedArticleId).then((detail) => {
        setSelectedArticleDetail(detail)
        setFormState({
          isActive: detail.isActive,
          title: detail.title,
          imageTag: detail.imageTag ?? '',
          positivityScore: detail.positivityScore ?? null,
          summaryShort: detail.summaryShort ?? '',
          contentRaw: detail.contentRaw ?? '',
          reason: '',
          note: '',
        })
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save moderation changes')
    } finally {
      setSubmitLoading(false)
    }
  }

  return (
    <>
        <div className="d-flex align-items-start justify-content-between mb-3">
          <div>
            <h2 className="h5 card-title mb-1">Moderation of articles</h2>
            <p className="small text-muted mb-0">Search by title or id and update article active state.</p>
          </div>
        </div>

        {error ? (
          <div className="alert alert-danger py-2" role="alert">
            {error}
          </div>
        ) : null}

        {submitMessage ? (
          <div className="alert alert-success py-2" role="status">
            {submitMessage}
          </div>
        ) : null}

        <form className="mb-3" onSubmit={handleSearch}>
          <div className="input-group">
            <input
              type="search"
              className="form-control"
              placeholder="Search articles by title or id"
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
            />
            <button type="submit" className="btn btn-outline-secondary" disabled={!token || loading}>
              {loading ? 'Searching…' : 'Search'}
            </button>
          </div>
        </form>
        <div className="mb-3">
          <button
            type="button"
            className="btn btn-sm btn-outline-secondary"
            onClick={handleClearSelection}
            disabled={!token || loading}
          >
            Clear
          </button>
        </div>

        {searchPerformed ? (
          <div className="row g-3">
            <div className="col-12">
              <div className="table-responsive border rounded mb-3" style={{ maxHeight: '26rem', overflowY: 'auto' }}>
                <table className="table table-sm table-hover mb-0">
                <thead className="table-light">
                  <tr>
                    <th scope="col">Id</th>
                    <th scope="col">Title</th>
                    <th scope="col">Source</th>
                    <th scope="col">Positivity</th>
                    <th scope="col">Active</th>
                    <th scope="col">ModeratedBy</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    <tr>
                      <td colSpan={6} className="text-muted">
                        Loading articles…
                      </td>
                    </tr>
                  ) : articles.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-muted">No articles found.</td>
                    </tr>
                  ) : (
                    articles.map((article) => (
                      <tr
                        key={article.id}
                        className={article.id === selectedArticleId ? 'table-primary' : undefined}
                        role="button"
                        onClick={() => handleSelectArticle(article.id)}
                      >
                        <td>{article.id}</td>
                        <td>{article.title}</td>
                        <td>{article.sourceName}</td>
                        <td>{article.positivityScore != null ? article.positivityScore.toFixed(2) : '-'}</td>
                        <td>{article.isActive ? 'Yes' : 'No'}</td>
                        <td>{formatModeratedBy(article.moderatedBy)}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
            <div className="border rounded p-3 mt-3">
              <div className="d-flex align-items-center justify-content-between mb-3">
                <h3 className="h6 mb-0">Moderation details</h3>
              </div>

              {selectedArticleId === null ? (
                <p className="text-muted">Select an article to review and update its active state.</p>
              ) : detailLoading ? (
                <p className="text-muted">Loading article details…</p>
              ) : selectedArticleDetail === null ? (
                <p className="text-muted">No article details available.</p>
              ) : (
                <form onSubmit={handleSubmit}>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="title">
                      Title
                    </label>
                    <input
                      id="title"
                      name="title"
                      type="text"
                      className="form-control"
                      value={formState.title ?? ''}
                      onChange={handleFormChange}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Source</label>
                    <input type="text" className="form-control" value={selectedArticleDetail.sourceName} disabled />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Published</label>
                    <input
                      type="text"
                      className="form-control"
                      value={formatApiUtcAsLocal(selectedArticleDetail.publishedAt)}
                      disabled
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="imageTag">
                      Image tag
                    </label>
                    <input
                      id="imageTag"
                      name="imageTag"
                      type="text"
                      className="form-control"
                      value={formState.imageTag ?? ''}
                      onChange={handleFormChange}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="positivityScore">
                      Positivity score
                    </label>
                    <input
                      id="positivityScore"
                      name="positivityScore"
                      type="number"
                      min="0"
                      max="1"
                      step="0.01"
                      className="form-control"
                      value={formState.positivityScore != null ? formState.positivityScore.toFixed(2) : ''}
                      onChange={handleFormChange}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="summaryShort">
                      Summary short
                    </label>
                    <textarea
                      id="summaryShort"
                      name="summaryShort"
                      className="form-control"
                      value={formState.summaryShort ?? ''}
                      onChange={handleFormChange}
                      rows={3}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="contentRaw">
                      Raw HTML content
                    </label>
                    <textarea
                      id="contentRaw"
                      name="contentRaw"
                      className="form-control"
                      value={formState.contentRaw ?? ''}
                      onChange={handleFormChange}
                      rows={8}
                    />
                  </div>
                  <div className="mb-3 form-check">
                    <input
                      id="isActive"
                      name="isActive"
                      type="checkbox"
                      className="form-check-input"
                      checked={formState.isActive}
                      onChange={handleFormChange}
                    />
                    <label className="form-check-label" htmlFor="isActive">
                      Is active
                    </label>
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="reason">
                      Reason
                    </label>
                    <textarea
                      id="reason"
                      name="reason"
                      className="form-control"
                      value={formState.reason ?? ''}
                      onChange={handleFormChange}
                      rows={2}
                      placeholder="Optional moderation reason"
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="note">
                      Notes
                    </label>
                    <textarea
                      id="note"
                      name="note"
                      className="form-control"
                      value={formState.note ?? ''}
                      onChange={handleFormChange}
                      rows={3}
                      placeholder="Optional note for audit log"
                    />
                  </div>
                  <div className="d-flex gap-2">
                    <button type="submit" className="btn btn-primary" disabled={submitLoading}>
                      {submitLoading ? 'Saving…' : 'Apply'}
                    </button>
                    <button type="button" className="btn btn-secondary" onClick={handleCancel} disabled={submitLoading}>
                      Cancel
                    </button>
                  </div>
                </form>
              )}
            </div>
          </div>
        </div>
        ) : null}
    </>
  )
}
