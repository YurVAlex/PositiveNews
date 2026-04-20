export type ArticlePreviewResponse = {
  id: number
  sourceName: string
  sourceLogoUrl: string | null
  title: string
  author: string | null
  publishedAt: string
  imageTag: string | null
  summaryShort: string
  topics: string[]
}

export type ArticleFeedResponse = {
  articles: ArticlePreviewResponse[]
  currentPage: number
  totalPages: number
  pageSize: number
  selectedTopic: string | null
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
