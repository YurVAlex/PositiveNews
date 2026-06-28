/** Admin comment review, including linked user complaints. */

import { apiUrl, authTokenHeader } from './http'

export type CommentComplaintAdminItem = {
  id: number
  userId: number
  userName: string
  reason: string
  createdAt: string
}

export type AdminCommentDetail = {
  id: number
  content: string
  createdAt: string
  userId: number
  userName: string
  isActive: boolean
  moderatedBy: number | null
  articleId: number
  complaints: CommentComplaintAdminItem[]
}

export type UpdateCommentRequest = {
  isActive: boolean
  reason?: string | null
  note?: string | null
}

export type CommentAdminItem = {
  id: number
  articleId: number
  userId: number
  complaintCount: number
  isActive: boolean
  moderatedBy: number | null
}

// Extract a human-readable message from ASP.NET ProblemDetails responses.
async function parseProblem(res: Response): Promise<string> {
  try {
    const body = (await res.json()) as { detail?: string; title?: string }
    return body.detail ?? body.title ?? `Request failed (${res.status})`
  } catch {
    return `Request failed (${res.status})`
  }
}

/** Loads a comment with its complaints so moderators can decide hide/restore actions. */
export async function fetchAdminCommentDetail(token: string, commentId: number): Promise<AdminCommentDetail> {
  const res = await fetch(apiUrl(`/api/admin/comments/${commentId}`), { headers: authTokenHeader(token) })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('Comment not found')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<AdminCommentDetail>
}

export async function fetchAdminComments(token: string): Promise<CommentAdminItem[]> {
  const res = await fetch(apiUrl('/api/admin/comments'), { headers: authTokenHeader(token) })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<CommentAdminItem[]>
}

/** Updates comment visibility and records moderation reason/note for the audit log. */
export async function updateAdminComment(token: string, commentId: number, payload: UpdateCommentRequest): Promise<void> {
  const res = await fetch(apiUrl(`/api/admin/comments/${commentId}`), {
    method: 'PUT',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('Comment not found')
  if (!res.ok) throw new Error(await parseProblem(res))
}
