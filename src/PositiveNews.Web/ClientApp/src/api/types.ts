export type ArticlePreviewResponse = {
  id: number
  sourceName: string
  sourceLogoUrl: string | null
  title: string
  author: string | null
  publishedAt: string
  imageTag: string | null
  summaryShort: string
  url: string
  positivityScore: number | null
  topics: string[]
}

export type ArticleFeedResponse = {
  articles: ArticlePreviewResponse[]
  currentPage: number
  totalPages: number
  pageSize: number
  /** Topics echoed from the request (trimmed, deduplicated); used for ordering only. */
  selectedTopics: string[]
}

export type ArticleDetailResponse = {
  id: number
  title: string
  sourceName: string
  sourceLogoUrl: string | null
  author: string | null
  publishedAt: string
  contentHtml: string | null
}
