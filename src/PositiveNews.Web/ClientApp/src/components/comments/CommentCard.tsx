/**
 * Single comment display with author, timestamp, and optional complain action.
 */
import type { CommentResponse } from '../../api/types'


type CommentCardProps = {
  comment: CommentResponse
  isAuthenticated: boolean
  onComplain: (comment: CommentResponse) => void
}

/** Formats comment created-at for the card header. */
function formatCommentDate(iso: string) {
  const d = new Date(iso)
  return d.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function CommentCard({ comment, isAuthenticated, onComplain }: CommentCardProps) {
  return (
    <div className="card card-body shadow-sm mb-3">
      <div className="d-flex flex-wrap align-items-center gap-2 mb-2">
        <span className="fw-bold">{comment.userName}</span>
        <span className="small text-muted">{formatCommentDate(comment.createdAt)}</span>
      </div>

      <p className="mb-0">{comment.content}</p>

      <div className="d-flex align-items-center gap-2 mt-2">
        {isAuthenticated ? (
          <button
            type="button"
            className="btn btn-outline-danger btn-sm"
            onClick={() => onComplain(comment)}
          >
            Complain
          </button>
        ) : null}
        <span className="small text-muted ms-auto">Comment id:{comment.id}</span>
      </div>
    </div>
  )
}
