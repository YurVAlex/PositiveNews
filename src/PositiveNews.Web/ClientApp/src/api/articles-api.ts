import type { ArticleDetailResponse, ArticleFeedResponse } from './types'

const apiBase = (import.meta.env.VITE_API_BASE ?? '').replace(/\/$/, '')

function apiUrl(path: string) {
  return `${apiBase}${path.startsWith('/') ? '' : '/'}${path}`
}

export async function fetchArticleFeed(page: number, topic: string | null): Promise<ArticleFeedResponse> {
  const params = new URLSearchParams()
  params.set('page', String(page))
  if (topic) {
    params.set('topic', topic)
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
