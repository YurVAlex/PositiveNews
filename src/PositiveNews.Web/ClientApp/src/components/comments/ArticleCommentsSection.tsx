/**
 * Article detail comments list with add and complain flows for authenticated users.
 */
import { useCallback, useEffect, useState } from 'react'

import { Link } from 'react-router-dom'
import { createArticleComment, fetchArticleComments, submitCommentComplaint } from '../../api/comments-api'
import type { CommentResponse } from '../../api/types'
import { useAuth } from '../../auth/AuthProvider'
import { AddCommentModal } from './AddCommentModal'
import { CommentCard } from './CommentCard'
import { ComplainModal } from './ComplainModal'

type ArticleCommentsSectionProps = {
  articleId: number
}

export function ArticleCommentsSection({ articleId }: ArticleCommentsSectionProps) {
  const { isAuthenticated, token } = useAuth()

  const [comments, setComments] = useState<CommentResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [isAddModalOpen, setIsAddModalOpen] = useState(false)
  const [isAddingComment, setIsAddingComment] = useState(false)
  const [addError, setAddError] = useState<string | null>(null)

  const [complainTarget, setComplainTarget] = useState<CommentResponse | null>(null)
  const [isSubmittingComplaint, setIsSubmittingComplaint] = useState(false)
  const [complainError, setComplainError] = useState<string | null>(null)
  const [complainSuccess, setComplainSuccess] = useState<string | null>(null)

  /** Loads active comments for the current article. */
  const loadComments = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)
    try {
      const response = await fetchArticleComments(articleId)
      setComments(response.comments)
    } catch (e) {
      setLoadError(e instanceof Error ? e.message : 'Failed to load comments')
      setComments([])
    } finally {
      setIsLoading(false)
    }
  }, [articleId])

  useEffect(() => {
    void loadComments()
  }, [loadComments])

  /** Posts a new comment and appends it to the local list. */
  const handleAddComment = async (content: string) => {
    if (!token) {
      return
    }

    setIsAddingComment(true)
    setAddError(null)
    try {
      const created = await createArticleComment(articleId, content, token)
      setComments((prev) => [...prev, created])
      setIsAddModalOpen(false)
    } catch (e) {
      setAddError(e instanceof Error ? e.message : 'Failed to post comment')
    } finally {
      setIsAddingComment(false)
    }
  }

  const handleOpenComplain = (comment: CommentResponse) => {
    setComplainTarget(comment)
    setComplainError(null)
    setComplainSuccess(null)
  }

  const handleCloseComplain = () => {
    setComplainTarget(null)
    setComplainError(null)
    setComplainSuccess(null)
  }

  /** Submits a complaint for the selected comment. */
  const handleSubmitComplaint = async (reason: string) => {
    if (!token || !complainTarget) {
      return
    }

    setIsSubmittingComplaint(true)
    setComplainError(null)
    try {
      await submitCommentComplaint(articleId, complainTarget.id, reason, token)
      setComplainSuccess('Your complaint has been submitted. Thank you.')
    } catch (e) {
      setComplainError(e instanceof Error ? e.message : 'Failed to submit complaint')
    } finally {
      setIsSubmittingComplaint(false)
    }
  }

  return (
    <section className="mt-5 pt-4 border-top" aria-label="Comments">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="h4 mb-0">Comments</h2>
        {isAuthenticated ? (
          <button
            type="button"
            className="btn btn-primary btn-sm"
            onClick={() => {
              setAddError(null)
              setIsAddModalOpen(true)
            }}
          >
            Leave a comment
          </button>
        ) : (
          <Link to="/login" className="btn btn-outline-primary btn-sm">
            Sign in to leave a comment
          </Link>
        )}
      </div>

      {isLoading ? (
        <div className="alert alert-secondary mb-0">Loading comments…</div>
      ) : null}

      {loadError ? <div className="alert alert-danger">{loadError}</div> : null}

      {!isLoading && !loadError && comments.length === 0 ? (
        <div className="alert alert-secondary mb-0">No comments yet. Be the first to share your thoughts.</div>
      ) : null}

      {!isLoading && !loadError
        ? comments.map((comment) => (
            <CommentCard
              key={comment.id}
              comment={comment}
              isAuthenticated={isAuthenticated}
              onComplain={handleOpenComplain}
            />
          ))
        : null}

      <AddCommentModal
        isOpen={isAddModalOpen}
        isSubmitting={isAddingComment}
        error={addError}
        onClose={() => setIsAddModalOpen(false)}
        onSubmit={handleAddComment}
      />

      <ComplainModal
        isOpen={complainTarget !== null}
        isSubmitting={isSubmittingComplaint}
        error={complainError}
        successMessage={complainSuccess}
        onClose={handleCloseComplain}
        onSubmit={handleSubmitComplaint}
      />
    </section>
  )
}
