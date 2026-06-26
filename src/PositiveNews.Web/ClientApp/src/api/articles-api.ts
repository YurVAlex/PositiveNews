/** Public article feed and detail endpoints for the reader UI. */

import type { ArticleDetailResponse, ArticleFeedResponse } from './types'

import { apiUrl, authTokenHeader } from './http'



export type FeedSortParam = 'date' | 'positivity' | 'preferences'



/**

 * Loads a paginated feed with optional topic/source filters.

 * Pass a token when using preference-based sort so the server can rank by saved interests.

 */

export async function fetchArticleFeed(

  page: number,

  topics: string[],

  sourceIds: number[],

  sort: FeedSortParam = 'date',

  token: string | null = null,

  minPositivity?: number,

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

  } else if (sort === 'preferences') {

    params.set('sort', 'preferences')

  }



  if (minPositivity !== undefined && Number.isFinite(minPositivity)) {

    params.set('minPositivity', String(minPositivity))

  }



  const res = await fetch(apiUrl(`/api/articles/feed?${params.toString()}`), {

    headers: authTokenHeader(token),

  })



  if (!res.ok) {

    throw new Error(`Feed request failed (${res.status})`)

  }



  return res.json() as Promise<ArticleFeedResponse>

}



/** Loads a single article; returns null on 404 so callers can render a not-found state without throwing. */

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

