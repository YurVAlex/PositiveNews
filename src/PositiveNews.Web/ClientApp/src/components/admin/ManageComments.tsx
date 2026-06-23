/**
 * Admin panel: look up a comment by id and toggle its active state.
 */
import { useState, type ChangeEvent, type FormEvent } from 'react'

import {
  fetchAdminCommentDetail,
  type AdminCommentDetail,
  type UpdateCommentRequest,
  updateAdminComment,
} from '../../api/admin-comments-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'

/** Validates and parses a positive integer comment id from search input. */
function parseCommentId(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed || !/^\d+$/.test(trimmed)) return null
  const id = Number(trimmed)
  return id > 0 ? id : null
}

export function ManageComments() {
  const { token } = useAuth()
  const [searchInput, setSearchInput] = useState('')
  const [commentDetail, setCommentDetail] = useState<AdminCommentDetail | null>(null)
  const [loading, setLoading] = useState(false)
  const [submitLoading, setSubmitLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)
  const [formState, setFormState] = useState<UpdateCommentRequest>({
    isActive: true,
    reason: '',
    note: '',
  })

  /** Loads comment detail by id for moderation. */
  const handleSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token) return

    const commentId = parseCommentId(searchInput)
    if (commentId === null) {
      setError('Enter a valid positive comment id.')
      setCommentDetail(null)
      return
    }

    setError(null)
    setSubmitMessage(null)
    setLoading(true)

    try {
      const detail = await fetchAdminCommentDetail(token, commentId)
      setCommentDetail(detail)
      setFormState({
        isActive: detail.isActive,
        reason: '',
        note: '',
      })
    } catch (err) {
      setCommentDetail(null)
      setError(err instanceof Error ? err.message : 'Search request failed')
    } finally {
      setLoading(false)
    }
  }

  const handleClose = () => {
    setCommentDetail(null)
    setSubmitMessage(null)
    setError(null)
    setFormState({
      isActive: true,
      reason: '',
      note: '',
    })
  }

  const handleFormChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement
    const value = target.type === 'checkbox'
      ? ('checked' in target ? target.checked : false)
      : target.value

    setFormState((current) => ({
      ...current,
      [target.name]: value,
    }))
  }

  /** Saves isActive and audit fields, then reloads comment detail. */
  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token || commentDetail === null) return

    setSubmitLoading(true)
    setError(null)
    setSubmitMessage(null)

    try {
      await updateAdminComment(token, commentDetail.id, formState)
      setSubmitMessage('Comment updated successfully.')
      const detail = await fetchAdminCommentDetail(token, commentDetail.id)
      setCommentDetail(detail)
      setFormState({
        isActive: detail.isActive,
        reason: '',
        note: '',
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save comment changes')
    } finally {
      setSubmitLoading(false)
    }
  }

  return (
    <section className="card mb-4">
      <div className="card-body">
        <div className="d-flex align-items-start justify-content-between mb-3">
          <div>
            <h2 className="h5 card-title mb-1">Moderation of comments</h2>
            <p className="small text-muted mb-0">Search by comment id</p>
          </div>
        </div>

        {error ? (
          <div className="alert alert-danger py-2" role="alert">{error}</div>
        ) : null}

        {submitMessage ? (
          <div className="alert alert-success py-2" role="status">{submitMessage}</div>
        ) : null}

        <form className="mb-3" onSubmit={handleSearch}>
          <div className="input-group">
            <input
              type="search"
              className="form-control"
              placeholder="Comment id"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
            />
            <button type="submit" className="btn btn-outline-secondary" disabled={!token || loading}>
              {loading ? 'Searching…' : 'Search'}
            </button>
          </div>
        </form>

        {commentDetail ? (
          <div className="border rounded p-3">
            <div className="d-flex align-items-center justify-content-between mb-3">
              <h3 className="h6 mb-0">Moderation details</h3>
            </div>

            <form onSubmit={handleSubmit}>
              <div className="mb-3">
                <label className="form-label">Content</label>
                <textarea className="form-control" value={commentDetail.content} disabled rows={4} />
              </div>
              <div className="mb-3">
                <label className="form-label">Created date</label>
                <input type="text" className="form-control" value={formatApiUtcAsLocal(commentDetail.createdAt)} disabled />
              </div>
              <div className="mb-3">
                <label className="form-label">User</label>
                <input type="text" className="form-control" value={commentDetail.userName} disabled />
              </div>
              <div className="mb-3">
                <label className="form-label">Complaints</label>
                {commentDetail.complaints.length === 0 ? (
                  <p className="text-muted small mb-0">No complaints filed.</p>
                ) : (
                  <ul className="list-unstyled mb-0">
                    {commentDetail.complaints.map((complaint) => (
                      <li key={complaint.id} className="mb-1">
                        <span>{complaint.userName}</span>
                        <span className="text-muted small ms-2">
                          {complaint.reason}
                          {' · '}
                          {formatApiUtcAsLocal(complaint.createdAt)}
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
              <div className="mb-3 form-check">
                <input id="commentIsActive" name="isActive" type="checkbox" className="form-check-input" checked={formState.isActive} onChange={handleFormChange} />
                <label className="form-check-label" htmlFor="commentIsActive">Is active</label>
              </div>
              <div className="mb-3">
                <label className="form-label" htmlFor="commentReason">Reason</label>
                <textarea id="commentReason" name="reason" className="form-control" value={formState.reason ?? ''} onChange={handleFormChange} rows={2} placeholder="Optional moderation reason" />
              </div>
              <div className="mb-3">
                <label className="form-label" htmlFor="commentNote">Notes</label>
                <textarea id="commentNote" name="note" className="form-control" value={formState.note ?? ''} onChange={handleFormChange} rows={3} placeholder="Optional note for audit log" />
              </div>
              <div className="d-flex gap-2">
                <button type="submit" className="btn btn-primary" disabled={submitLoading}>{submitLoading ? 'Applying…' : 'Apply'}</button>
                <button type="button" className="btn btn-outline-secondary" onClick={handleClose} disabled={submitLoading}>Close</button>
              </div>
            </form>
          </div>
        ) : (
          <p className="text-muted mb-0">Search for a comment to review and update its state.</p>
        )}
      </div>
    </section>
  )
}
