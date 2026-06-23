/**
 * Modal form for filing a complaint against a comment.
 */
import { type FormEvent, useState } from 'react'

import { COMPLAINT_REASON_MAX_LENGTH, getComplaintReasonError } from '../../utils/comment-validation'

type ComplainModalProps = {
  isOpen: boolean
  isSubmitting: boolean
  error: string | null
  successMessage: string | null
  onClose: () => void
  onSubmit: (reason: string) => void
}

export function ComplainModal({
  isOpen,
  isSubmitting,
  error,
  successMessage,
  onClose,
  onSubmit,
}: ComplainModalProps) {
  const [reason, setReason] = useState('')
  const [reasonError, setReasonError] = useState<string | null>(null)

  if (!isOpen) {
    return null
  }

  /** Clears local state and notifies parent to close the modal. */
  const handleClose = () => {
    setReason('')
    setReasonError(null)
    onClose()
  }

  /** Validates reason length and delegates to parent onSubmit. */
  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    const validationError = getComplaintReasonError(reason)
    setReasonError(validationError)
    if (validationError) {
      return
    }
    onSubmit(reason.trim())
  }

  return (
    <>
      <div className="modal fade show d-block" tabIndex={-1} role="dialog" aria-modal="true">
        <div className="modal-dialog">
          <div className="modal-content">
            <form onSubmit={handleSubmit} noValidate>
              <div className="modal-header">
                <h5 className="modal-title">File a complaint</h5>
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
                {successMessage ? <div className="alert alert-success">{successMessage}</div> : null}
                <div className="mb-3">
                  <label htmlFor="complaint-reason" className="form-label">
                    Reason
                  </label>
                  <textarea
                    id="complaint-reason"
                    className={['form-control', reasonError ? 'is-invalid' : ''].filter(Boolean).join(' ')}
                    rows={4}
                    maxLength={COMPLAINT_REASON_MAX_LENGTH}
                    value={reason}
                    onChange={(e) => {
                      setReason(e.target.value)
                      if (reasonError) {
                        setReasonError(getComplaintReasonError(e.target.value))
                      }
                    }}
                    aria-invalid={reasonError ? true : undefined}
                    disabled={isSubmitting || !!successMessage}
                  />
                  {reasonError ? (
                    <div className="invalid-feedback d-block">{reasonError}</div>
                  ) : (
                    <div className="form-text">
                      {reason.trim().length}/{COMPLAINT_REASON_MAX_LENGTH} characters
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
                  {successMessage ? 'Close' : 'Cancel'}
                </button>
                {!successMessage ? (
                  <button type="submit" className="btn btn-danger" disabled={isSubmitting}>
                    {isSubmitting ? 'Sending…' : 'Submit complaint'}
                  </button>
                ) : null}
              </div>
            </form>
          </div>
        </div>
      </div>
      <div className="modal-backdrop fade show" />
    </>
  )
}
