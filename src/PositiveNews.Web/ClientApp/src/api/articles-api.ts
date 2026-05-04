import type { ArticleDetailResponse, ArticleFeedResponse } from './types'

const apiBase = (import.meta.env.VITE_API_BASE ?? '').replace(/\/$/, '')

function apiUrl(path: string) {
  return `${apiBase}${path.startsWith('/') ? '' : '/'}${path}`
}

export type FeedSortParam = 'date' | 'positivity'

export async function fetchArticleFeed(
  page: number,
  topics: string[],
  sort: FeedSortParam = 'date',
): Promise<ArticleFeedResponse> {
  const params = new URLSearchParams()
  params.set('page', String(page))
  for (const t of topics) {
    const trimmed = t.trim()
    if (trimmed.length > 0) {
      params.append('topic', trimmed)
    }
  }

  if (sort === 'positivity') {
    params.set('sort', 'positivity')
  }

  const res = await fetch(apiUrl(`/api/articles/feed?${params.toString()}`), {
    headers: { Accept: 'application/json' },
  })

  if (!res.ok) {
    throw new Error(`Feed request failed (${res.status})`)
  }

  return res.json() as Promise<ArticleFeedResponse>
}

export async function fetchArticleDetail(id: number): Promise<ArticleDetailResponse | null> {
  const res = await fetch(apiUrl(`/api/articles/${id}`), {
    headers: { Accept: 'application/json' },
  })

  if (res.status === 404) {
    return null
  }

  if (!res.ok) {
    throw new Error(`Article request failed (${res.status})`)
  }

  return res.json() as Promise<ArticleDetailResponse>
}
