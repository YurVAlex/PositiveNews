import type { ArticleDetailResponse, ArticleFeedResponse } from './types'
import { apiUrl, authTokenHeader } from './http'

export type FeedSortParam = 'date' | 'positivity'

export async function fetchArticleFeed(
  page: number,
  topics: string[],
  sourceIds: number[],
  sort: FeedSortParam = 'date',
  token: string | null = null,
): Promise<ArticleFeedResponse> {
  const params = new URLSearchParams()
  params.set('page', String(page))
  for (const t of topics) {
    const trimmed = t.trim()
    if (trimmed.length > 0) {
      params.append('topic', trimmed)
    }
  }
  for (const id of sourceIds) {
    if (Number.isInteger(id) && id > 0) {
      params.append('source', String(id))
    }
  }

  if (sort === 'positivity') {
    params.set('sort', 'positivity')
  }

  const res = await fetch(apiUrl(`/api/articles/feed?${params.toString()}`), {
    headers: authTokenHeader(token),
  })

  if (!res.ok) {
    throw new Error(`Feed request failed (${res.status})`)
  }

  return res.json() as Promise<ArticleFeedResponse>
}

export async function fetchArticleDetail(id: number, token: string | null = null): Promise<ArticleDetailResponse | null> {
  const res = await fetch(apiUrl(`/api/articles/${id}`), {
    headers: authTokenHeader(token),
  })

  if (res.status === 404) {
    return null
  }

  if (!res.ok) {
    throw new Error(`Article request failed (${res.status})`)
  }

  return res.json() as Promise<ArticleDetailResponse>
}
