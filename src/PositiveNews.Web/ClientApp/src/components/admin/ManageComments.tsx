/**
 * Admin panel: search comments, review details, and deactivate active comments.
 */
import { useCallback, useEffect, useState, type ChangeEvent, type FormEvent } from 'react'

import {
  fetchAdminCommentDetail,
  fetchAdminComments,
  type AdminCommentDetail,
  type CommentAdminItem,
  type UpdateCommentRequest,
  updateAdminComment,
} from '../../api/admin-comments-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'
import { formatModeratedBy } from '../../utils/format-moderated-by'

function parseCommentId(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed || !/^\d+$/.test(trimmed)) return null
  const id = Number(trimmed)
  return id > 0 ? id : null
}

export function ManageComments() {
  const { token } = useAuth()
  const [searchInput, setSearchInput] = useState('')
  const [comments, setComments] = useState<CommentAdminItem[]>([])
  const [tableLoading, setTableLoading] = useState(false)
  const [selectedCommentId, setSelectedCommentId] = useState<number | null>(null)
  const [commentDetail, setCommentDetail] = useState<AdminCommentDetail | null>(null)
  const [searchLoading, setSearchLoading] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)
  const [submitLoading, setSubmitLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)
  const [formState, setFormState] = useState<UpdateCommentRequest>({
    isActive: true,
    reason: '',
    note: '',
  })

  const loadCommentsTable = useCallback(async () => {
    if (!token) return
    setTableLoading(true)
    setError(null)
    try {
      const items = await fetchAdminComments(token)
      setComments(items)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load comments')
    } finally {
      setTableLoading(false)
    }
  }, [token])

  const loadCommentDetail = useCallback(async (commentId: number) => {
    if (!token) return
    setDetailLoading(true)
    setError(null)
    try {
      const detail = await fetchAdminCommentDetail(token, commentId)
      setCommentDetail(detail)
      setSelectedCommentId(commentId)
      setFormState({
        isActive: detail.isActive,
        reason: '',
        note: '',
      })
    } catch (err) {
      setCommentDetail(null)
      setError(err instanceof Error ? err.message : 'Failed to load comment details')
    } finally {
      setDetailLoading(false)
    }
  }, [token])

  useEffect(() => {
    void loadCommentsTable()
  }, [loadCommentsTable])

  useEffect(() => {
    if (selectedCommentId === null || !token) {
      setCommentDetail(null)
      return
    }

    void loadCommentDetail(selectedCommentId)
  }, [selectedCommentId, token, loadCommentDetail])

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token) return

    const commentId = parseCommentId(searchInput)
    if (commentId === null) {
      setError('Enter a valid positive comment id.')
      setCommentDetail(null)
      setSelectedCommentId(null)
      return
    }

    setError(null)
    setSubmitMessage(null)
    setSearchLoading(true)
    setSelectedCommentId(commentId)
    setSearchLoading(false)
  }

  const handleSelectComment = (id: number) => {
    setSelectedCommentId(id)
    setSubmitMessage(null)
    setError(null)
  }

  const handleCloseDetail = () => {
    setSelectedCommentId(null)
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

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token || commentDetail === null) return

    setSubmitLoading(true)
    setError(null)
    setSubmitMessage(null)

    try {
      const wasActive = formState.isActive
      await updateAdminComment(token, commentDetail.id, formState)
      await loadCommentsTable()
      if (wasActive) {
        await loadCommentDetail(commentDetail.id)
      } else {
        setSelectedCommentId(null)
        setCommentDetail(null)
        setFormState({
          isActive: true,
          reason: '',
          note: '',
        })
      }
      setSubmitMessage('Comment updated successfully.')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save comment changes')
    } finally {
      setSubmitLoading(false)
    }
  }

  return (
    <>
      <div className="d-flex align-items-start justify-content-between mb-3">
        <div>
          <h2 className="h5 card-title mb-1">Moderation of comments</h2>
          <p className="small text-muted mb-0">Search by comment id or select from active comments.</p>
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
          <button type="submit" className="btn btn-outline-secondary" disabled={!token || searchLoading}>
            {searchLoading ? 'Searching…' : 'Search'}
          </button>
        </div>
      </form>

      {selectedCommentId !== null ? (
        <div className="border rounded p-3 mb-3">
          <div className="d-flex align-items-center justify-content-between mb-3">
            <h3 className="h6 mb-0">Moderation details</h3>
          </div>

          {detailLoading ? (
            <p className="text-muted">Loading comment details…</p>
          ) : commentDetail === null ? (
            <p className="text-muted">No comment details available.</p>
          ) : (
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
                <button type="button" className="btn btn-outline-secondary" onClick={handleCloseDetail} disabled={submitLoading}>Close</button>
              </div>
            </form>
          )}
        </div>
      ) : null}

      {tableLoading ? (
        <p className="text-muted">Loading comments…</p>
      ) : null}

      <div className="table-responsive border rounded" style={{ maxHeight: '26rem', overflowY: 'auto' }}>
        <table className="table table-sm table-hover mb-0">
          <thead className="table-light sticky-top">
            <tr>
              <th scope="col">ID</th>
              <th scope="col">Article ID</th>
              <th scope="col">User ID</th>
              <th scope="col">Complains</th>
              <th scope="col">ModeratedBy</th>
            </tr>
          </thead>
          <tbody>
            {comments.length === 0 ? (
              <tr><td colSpan={5} className="text-muted">No active comments found.</td></tr>
            ) : (
              comments.map((comment) => (
                <tr
                  key={comment.id}
                  className={comment.id === selectedCommentId ? 'table-primary' : undefined}
                  role="button"
                  onClick={() => handleSelectComment(comment.id)}
                >
                  <td>{comment.id}</td>
                  <td>{comment.articleId}</td>
                  <td>{comment.userId}</td>
                  <td>{comment.complaintCount}</td>
                  <td>{formatModeratedBy(comment.moderatedBy)}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </>
  )
}
