/** Admin article listing, detail, and moderation updates. */

import { apiUrl, authTokenHeader } from './http'

export type AdminArticleItem = {
  id: number
  sourceId: number
  sourceName: string
  title: string
  positivityScore: number | null
  isActive: boolean
  moderatedBy: number | null
  publishedAt: string
}

export type AdminArticleDetail = AdminArticleItem & {
  sourceLogoUrl: string | null
  author: string | null
  url: string
  summaryShort: string
  imageTag: string | null
  contentRaw: string | null
}

export type ArticleModerationRequest = {
  isActive: boolean
  title?: string | null
  imageTag?: string | null
  positivityScore?: number | null
  summaryShort?: string | null
  contentRaw?: string | null
  reason?: string | null
  note?: string | null
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

/** Lists articles for the admin table; optional search narrows by title. */
export async function fetchAdminArticles(token: string, searchTerm?: string): Promise<AdminArticleItem[]> {
  const uri = searchTerm ? apiUrl(`/api/admin/articles?q=${encodeURIComponent(searchTerm)}`) : apiUrl('/api/admin/articles')
  const res = await fetch(uri, { headers: authTokenHeader(token) })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<AdminArticleItem[]>
}

/** Loads full article content and moderation metadata for the admin detail view. */
export async function fetchAdminArticleDetail(token: string, articleId: number): Promise<AdminArticleDetail> {
  const res = await fetch(apiUrl(`/api/admin/articles/${articleId}`), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('Article not found')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<AdminArticleDetail>
}

/** Applies moderation changes (visibility, content edits, scores) and records audit metadata. */
export async function moderateArticle(
  token: string,
  articleId: number,
  payload: ArticleModerationRequest,
): Promise<void> {
  const res = await fetch(apiUrl(`/api/admin/articles/${articleId}`), {
    method: 'PUT',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('Article not found')
  if (!res.ok) throw new Error(await parseProblem(res))
}
