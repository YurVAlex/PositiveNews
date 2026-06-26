import { fetchArticleDetail, fetchArticleFeed } from './articles-api'
import type { ArticleDetailResponse, ArticleFeedResponse } from './types'

const fetchMock = vi.fn()

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('fetchArticleFeed', () => {
  it('serializes page, topics, sources, positivity sort, and auth header', async () => {
    const response: ArticleFeedResponse = {
      articles: [],
      currentPage: 2,
      totalPages: 3,
      pageSize: 10,
      selectedTopics: ['Health'],
      selectedSources: [],
    }
    fetchMock.mockResolvedValue(okResponse(response))

    await fetchArticleFeed(2, ['Health', ' ', 'Science'], [1, 0, 2], 'positivity', 'token')

    expect(fetchMock).toHaveBeenCalledTimes(1)
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit]
    const parsed = new URL(url, 'http://localhost')
    expect(parsed.pathname).toBe('/api/articles/feed')
    expect(parsed.searchParams.get('page')).toBe('2')
    expect(parsed.searchParams.getAll('topic')).toEqual(['Health', 'Science'])
    expect(parsed.searchParams.getAll('source')).toEqual(['1', '2'])
    expect(parsed.searchParams.get('sort')).toBe('positivity')
    expect(init.headers).toEqual({
      Accept: 'application/json',
      Authorization: 'Bearer token',
    })
  })

  it('serializes preferences sort when requested', async () => {
    fetchMock.mockResolvedValue(
      okResponse({
        articles: [],
        currentPage: 1,
        totalPages: 1,
        pageSize: 10,
        selectedTopics: ['Health'],
        selectedSources: [],
      }),
    )

    await fetchArticleFeed(1, ['Health'], [], 'preferences')

    const [url] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(new URL(url, 'http://localhost').searchParams.get('sort')).toBe('preferences')
  })

  it('serializes minPositivity when provided', async () => {
    fetchMock.mockResolvedValue(
      okResponse({
        articles: [],
        currentPage: 1,
        totalPages: 1,
        pageSize: 10,
        selectedTopics: [],
        selectedSources: [],
      }),
    )

    await fetchArticleFeed(1, [], [], 'date', null, 0.65)

    const [url] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(new URL(url, 'http://localhost').searchParams.get('minPositivity')).toBe('0.65')
  })

  it('omits sort when date sort is requested', async () => {
    fetchMock.mockResolvedValue(
      okResponse({
        articles: [],
        currentPage: 1,
        totalPages: 1,
        pageSize: 10,
        selectedTopics: [],
        selectedSources: [],
      }),
    )

    await fetchArticleFeed(1, [], [], 'date')

    const [url] = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(new URL(url, 'http://localhost').searchParams.has('sort')).toBe(false)
  })

  it('throws when the feed request fails', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 500 }))

    await expect(fetchArticleFeed(1, [], [])).rejects.toThrow('Feed request failed (500)')
  })
})

describe('fetchArticleDetail', () => {
  it('returns null for not found details', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 404 }))

    await expect(fetchArticleDetail(123)).resolves.toBeNull()
  })

  it('returns detail JSON for successful response', async () => {
    const detail: ArticleDetailResponse = {
      id: 1,
      title: 'Title',
      sourceName: 'Source',
      sourceLogoUrl: null,
      author: null,
      publishedAt: '2026-05-11T00:00:00Z',
      contentHtml: '<p>Content</p>',
    }
    fetchMock.mockResolvedValue(okResponse(detail))

    await expect(fetchArticleDetail(1, 'token')).resolves.toEqual(detail)
    expect(fetchMock).toHaveBeenCalledWith('/api/articles/1', {
      headers: {
        Accept: 'application/json',
        Authorization: 'Bearer token',
      },
    })
  })
})

function okResponse(body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status: 200,
    headers: { 'Content-Type': 'application/json' },
  })
}
