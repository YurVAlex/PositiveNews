/**
 * Client-side validation for comment and complaint form fields.
 * Mirrors server length limits so users see errors before submit.
 */

export const COMMENT_MAX_LENGTH = 2000
export const COMPLAINT_REASON_MAX_LENGTH = 500

/** Returns a user-facing error for comment body, or null when valid. */
export function getCommentContentError(content: string): string | null {
  if (!content.trim()) {
    return 'Comment cannot be empty.'
  }
  if (content.trim().length > COMMENT_MAX_LENGTH) {
    return `Comment cannot exceed ${COMMENT_MAX_LENGTH} characters.`
  }
  return null
}

/** Returns a user-facing error for complaint reason text, or null when valid. */
export function getComplaintReasonError(reason: string): string | null {
  if (!reason.trim()) {
    return 'Complaint reason cannot be empty.'
  }
  if (reason.trim().length > COMPLAINT_REASON_MAX_LENGTH) {
    return `Complaint reason cannot exceed ${COMPLAINT_REASON_MAX_LENGTH} characters.`
  }
  return null
}
