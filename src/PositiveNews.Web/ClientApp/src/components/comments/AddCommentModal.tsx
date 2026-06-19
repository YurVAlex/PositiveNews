import { type FormEvent, useState } from 'react'
import { COMMENT_MAX_LENGTH, getCommentContentError } from '../../utils/comment-validation'

type AddCommentModalProps = {
  isOpen: boolean
  isSubmitting: boolean
  error: string | null
  onClose: () => void
  onSubmit: (content: string) => void
}

export function AddCommentModal({ isOpen, isSubmitting, error, onClose, onSubmit }: AddCommentModalProps) {
  const [content, setContent] = useState('')
  const [contentError, setContentError] = useState<string | null>(null)

  if (!isOpen) {
    return null
  }

  const handleClose = () => {
    setContent('')
    setContentError(null)
    onClose()
  }

  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const validationError = getCommentContentError(content)
    setContentError(validationError)
    if (validationError) {
      return
    }
    onSubmit(content.trim())
  }

  return (
    <>
      <div className="modal fade show d-block" tabIndex={-1} role="dialog" aria-modal="true">
        <div className="modal-dialog">
          <div className="modal-content">
            <form onSubmit={handleSubmit} noValidate>
              <div className="modal-header">
                <h5 className="modal-title">Leave a comment</h5>
                <button
                  type="button"
                  className="btn-close"
                  aria-label="Close"
                  onClick={handleClose}
                  disabled={isSubmitting}
                />
              </div>
              <div className="modal-body">
                {error ? <div className="alert alert-danger">{error}</div> : null}
                <div className="mb-3">
                  <label htmlFor="comment-content" className="form-label">
                    Your comment
                  </label>
                  <textarea
                    id="comment-content"
                    className={['form-control', contentError ? 'is-invalid' : ''].filter(Boolean).join(' ')}
                    rows={4}
                    maxLength={COMMENT_MAX_LENGTH}
                    value={content}
                    onChange={(e) => {
                      setContent(e.target.value)
                      if (contentError) {
                        setContentError(getCommentContentError(e.target.value))
                      }
                    }}
                    aria-invalid={contentError ? true : undefined}
                    disabled={isSubmitting}
                  />
                  {contentError ? (
                    <div className="invalid-feedback d-block">{contentError}</div>
                  ) : (
                    <div className="form-text">
                      {content.trim().length}/{COMMENT_MAX_LENGTH} characters
                    </div>
                  )}
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={handleClose}
                  disabled={isSubmitting}
                >
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
                  {isSubmitting ? 'Sending…' : 'Send comment'}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
      <div className="modal-backdrop fade show" />
    </>
  )
}
