/**
 * Admin panel: browse sources, edit trust score/feed URL, and toggle active state.
 */
import { useCallback, useEffect, useMemo, useState, type ChangeEvent, type FormEvent } from 'react'

import {
  fetchAdminSources,
  fetchSourceDetail,
  type SourceAdminDetail,
  type SourceAdminItem,
  type UpdateSourceRequest,
  updateSource,
} from '../../api/admin-sources-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatModeratedBy } from '../../utils/format-moderated-by'

export function SourcesModeration() {
  const { token } = useAuth()
  const [sources, setSources] = useState<SourceAdminItem[]>([])
  const [selectedSourceId, setSelectedSourceId] = useState<number | null>(null)
  const [detail, setDetail] = useState<SourceAdminDetail | null>(null)
  const [loading, setLoading] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)
  const [submitLoading, setSubmitLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)
  const [formState, setFormState] = useState<UpdateSourceRequest>({
    trustScore: 0,
    isActive: false,
    feedUrl: '',
    reason: '',
    note: '',
  })

  const selectedSource = useMemo(
    () => sources.find((item) => item.id === selectedSourceId) ?? null,
    [selectedSourceId, sources],
  )

  const loadSources = useCallback(async () => {
    if (!token) return
    setLoading(true)
    setError(null)
    try {
      const items = await fetchAdminSources(token)
      setSources(items)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load sources')
    } finally {
      setLoading(false)
    }
  }, [token])

  useEffect(() => {
    void loadSources()
  }, [loadSources])

  useEffect(() => {
    if (selectedSourceId === null || !token) {
      setDetail(null)
      return
    }

    setDetailLoading(true)
    setError(null)
    void fetchSourceDetail(token, selectedSourceId)
      .then((item) => {
        setDetail(item)
        setFormState({
          trustScore: item.trustScore,
          isActive: item.isActive,
          feedUrl: item.feedUrl,
          reason: '',
          note: '',
        })
        setSubmitMessage(null)
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load source details'))
      .finally(() => setDetailLoading(false))
  }, [selectedSourceId, token])

  const handleSelectSource = (id: number) => {
    setSelectedSourceId(id)
    setSubmitMessage(null)
  }

  const handleCloseDetail = () => {
    setSelectedSourceId(null)
    setDetail(null)
    setSubmitMessage(null)
    setError(null)
    setFormState({
      trustScore: 0,
      isActive: false,
      feedUrl: '',
      reason: '',
      note: '',
    })
  }

  const handleFormChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement
    const value = target.value
    const updatedValue =
      target.type === 'checkbox' && 'checked' in target
        ? target.checked
        : target.name === 'trustScore'
        ? parseFloat(value) ?? 0
        : value

    setFormState((current) => ({
      ...current,
      [target.name]: updatedValue,
    }))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token || selectedSourceId === null) return

    setSubmitLoading(true)
    setError(null)
    setSubmitMessage(null)

    try {
      await updateSource(token, selectedSourceId, formState)
      await loadSources()
      handleCloseDetail()
      setSubmitMessage('Source updated successfully.')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update source')
    } finally {
      setSubmitLoading(false)
    }
  }

  return (
    <>
      <h2 className="h5 card-title mb-3">Moderation of sources</h2>

      {error ? (
        <div className="alert alert-danger py-2" role="alert">{error}</div>
      ) : null}

      {submitMessage ? (
        <div className="alert alert-success py-2" role="status">{submitMessage}</div>
      ) : null}

      <div className="table-responsive border rounded mb-3" style={{ maxHeight: '26rem', overflowY: 'auto' }}>
        <table className="table table-sm table-hover mb-0">
          <thead className="table-light sticky-top">
            <tr>
              <th scope="col">Id</th>
              <th scope="col">Name</th>
              <th scope="col">ModeratedBy</th>
              <th scope="col">Active</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={4} className="text-muted">Loading sources…</td></tr>
            ) : sources.length === 0 ? (
              <tr><td colSpan={4} className="text-muted">No sources found.</td></tr>
            ) : (
              sources.map((source) => (
                <tr
                  key={source.id}
                  className={source.id === selectedSourceId ? 'table-primary' : undefined}
                  role="button"
                  onClick={() => handleSelectSource(source.id)}
                >
                  <td>{source.id}</td>
                  <td>{source.name}</td>
                  <td>{formatModeratedBy(source.moderatedBy)}</td>
                  <td>{source.isActive ? 'Yes' : 'No'}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="border rounded p-3">
        <h3 className="h6 mb-3">Edit selected source</h3>

        {selectedSource === null ? (
          <p className="text-muted mb-0">Select a source to view details and make changes.</p>
        ) : detailLoading ? (
          <p className="text-muted mb-0">Loading source details…</p>
        ) : detail === null ? (
          <p className="text-muted mb-0">No details available.</p>
        ) : (
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Source</label>
              <input type="text" className="form-control" value={detail.name} disabled />
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="trustScore">Trust score</label>
              <input
                id="trustScore"
                name="trustScore"
                type="number"
                step="0.01"
                min="0"
                max="1"
                className="form-control"
                value={formState.trustScore}
                onChange={handleFormChange}
                required
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
              <label className="form-check-label" htmlFor="isActive">Is active</label>
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="feedUrl">Feed URL</label>
              <input
                id="feedUrl"
                name="feedUrl"
                type="url"
                className="form-control"
                value={formState.feedUrl}
                onChange={handleFormChange}
                required
              />
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="reason">Reason</label>
              <textarea id="reason" name="reason" className="form-control" value={formState.reason ?? ''} onChange={handleFormChange} rows={2} />
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="note">Note</label>
              <textarea id="note" name="note" className="form-control" value={formState.note ?? ''} onChange={handleFormChange} rows={3} />
            </div>
            <button type="submit" className="btn btn-primary" disabled={submitLoading}>
              {submitLoading ? 'Saving…' : 'Save changes'}
            </button>
          </form>
        )}
      </div>
    </>
  )
}
