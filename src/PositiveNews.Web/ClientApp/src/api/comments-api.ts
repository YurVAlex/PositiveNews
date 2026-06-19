import { apiUrl, authTokenHeader } from './http'
import type { ArticleCommentsListResponse, CommentResponse } from './types'

type ApiProblemDetails = {
  title?: string
  detail?: string
}

async function readErrorMessage(res: Response, fallback: string): Promise<string> {
  try {
    const data = (await res.json()) as ApiProblemDetails
    if (typeof data.detail === 'string' && data.detail.trim().length > 0) {
      return data.detail
    }
    if (typeof data.title === 'string' && data.title.trim().length > 0) {
      return data.title
    }
  } catch {
    // Ignore parse errors and use fallback message.
  }

  return fallback
}

export async function fetchArticleComments(articleId: number): Promise<ArticleCommentsListResponse> {
  const res = await fetch(apiUrl(`/api/articles/${articleId}/comments`), {
    headers: { Accept: 'application/json' },
  })

  if (!res.ok) {
    throw new Error(await readErrorMessage(res, `Failed to load comments (${res.status})`))
  }

  return res.json() as Promise<ArticleCommentsListResponse>
}

export async function createArticleComment(
  articleId: number,
  content: string,
  token: string
): Promise<CommentResponse> {
  const res = await fetch(apiUrl(`/api/articles/${articleId}/comments`), {
    method: 'POST',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ content }),
  })

  if (!res.ok) {
    if (res.status === 400) {
      throw new Error(await readErrorMessage(res, 'Comment data is invalid.'))
    }
    if (res.status === 401) {
      throw new Error(await readErrorMessage(res, 'You must be signed in to leave a comment.'))
    }
    if (res.status === 404) {
      throw new Error(await readErrorMessage(res, 'Article not found.'))
    }
    throw new Error(await readErrorMessage(res, `Failed to post comment (${res.status})`))
  }

  return res.json() as Promise<CommentResponse>
}

export async function submitCommentComplaint(
  articleId: number,
  commentId: number,
  reason: string,
  token: string
): Promise<void> {
  const res = await fetch(apiUrl(`/api/articles/${articleId}/comments/${commentId}/complains`), {
    method: 'POST',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ reason }),
  })

  if (!res.ok) {
    if (res.status === 400) {
      throw new Error(await readErrorMessage(res, 'Complaint data is invalid.'))
    }
    if (res.status === 401) {
      throw new Error(await readErrorMessage(res, 'You must be signed in to file a complaint.'))
    }
    if (res.status === 404) {
      throw new Error(await readErrorMessage(res, 'Comment not found.'))
    }
    if (res.status === 409) {
      throw new Error(await readErrorMessage(res, 'You have already filed a complaint against this comment.'))
    }
    throw new Error(await readErrorMessage(res, `Failed to submit complaint (${res.status})`))
  }
}
