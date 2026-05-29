import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import {
  fetchAdminArticles,
  fetchAdminArticleDetail,
  moderateArticle,
  type AdminArticleDetail,
  type AdminArticleItem,
  type ModerateArticleRequest,
} from '../../api/admin-articles-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'

export function ModerateArticle() {
  const { token } = useAuth()
  const [articles, setArticles] = useState<AdminArticleItem[]>([])
  const [selectedArticleId, setSelectedArticleId] = useState<number | null>(null)
  const [selectedArticleDetail, setSelectedArticleDetail] = useState<AdminArticleDetail | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [loading, setLoading] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)
  const [submitLoading, setSubmitLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)
  const [formState, setFormState] = useState<ModerateArticleRequest>({
    isActive: false,
    reason: '',
    note: '',
  })

  useEffect(() => {
    if (!token) return
    setError(null)
    setLoading(true)
    void fetchAdminArticles(token)
      .then((items) => setArticles(items))
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load articles'))
      .finally(() => setLoading(false))
  }, [token])

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
          reason: '',
          note: '',
        })
        setSubmitMessage(null)
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load article details'))
      .finally(() => setDetailLoading(false))
  }, [selectedArticleId, token])

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

  const handleFormChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement
    const value = target.type === 'checkbox' && 'checked' in target ? target.checked : target.value

    setFormState((current) => ({
      ...current,
      [target.name]: value,
    }))
  }

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
    <section className="card mb-4">
      <div className="card-body">
        <div className="d-flex align-items-start justify-content-between mb-3">
          <div>
            <h2 className="h5 card-title mb-1">Moderate articles</h2>
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

        <div className="row g-3">
          <div className="col-12 col-lg-6">
            <div className="table-responsive border rounded mb-3" style={{ maxHeight: '26rem', overflowY: 'auto' }}>
              <table className="table table-sm table-hover mb-0">
                <thead className="table-light">
                  <tr>
                    <th scope="col">Id</th>
                    <th scope="col">Title</th>
                    <th scope="col">Source</th>
                    <th scope="col">Active</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    <tr>
                      <td colSpan={4} className="text-muted">
                        Loading articles…
                      </td>
                    </tr>
                  ) : articles.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="text-muted">No articles found.</td>
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
                        <td>{article.isActive ? 'Yes' : 'No'}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>

          <div className="col-12 col-lg-6">
            <div className="border rounded p-3">
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
                    <label className="form-label">Title</label>
                    <input type="text" className="form-control" value={selectedArticleDetail.title} disabled />
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
                  <button type="submit" className="btn btn-primary" disabled={submitLoading}>
                    {submitLoading ? 'Saving…' : 'Save moderation'}
                  </button>
                </form>
              )}
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
