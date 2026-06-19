export const COMMENT_MAX_LENGTH = 2000
export const COMPLAINT_REASON_MAX_LENGTH = 500

export function getCommentContentError(content: string): string | null {
  if (!content.trim()) {
    return 'Comment cannot be empty.'
  }
  if (content.trim().length > COMMENT_MAX_LENGTH) {
    return `Comment cannot exceed ${COMMENT_MAX_LENGTH} characters.`
  }
  return null
}

export function getComplaintReasonError(reason: string): string | null {
  if (!reason.trim()) {
    return 'Complaint reason cannot be empty.'
  }
  if (reason.trim().length > COMPLAINT_REASON_MAX_LENGTH) {
    return `Complaint reason cannot exceed ${COMPLAINT_REASON_MAX_LENGTH} characters.`
  }
  return null
}
