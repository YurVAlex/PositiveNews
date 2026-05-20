export type SourceFilterItem = {
  id: number
  name: string
  logoUrl: string | null
}

export type SourcesMetadataResponse = {
  sources: SourceFilterItem[]
}

export type TopicsMetadataResponse = {
  topicNames: string[]
}

export type ArticlePreviewResponse = {
  id: number
  sourceId: number
  sourceName: string
  sourceLogoUrl: string | null
  /** Editorial trust weight from the news source (API decimal). */
  sourceTrustScore: number
  title: string
  author: string | null
  publishedAt: string
  imageTag: string | null
  summaryShort: string
  url: string
  positivityScore: number | null
  viewCount: number
  topics: string[]
}

export type ArticleFeedResponse = {
  articles: ArticlePreviewResponse[]
  currentPage: number
  totalPages: number
  pageSize: number
  /** Topics echoed from the request (trimmed, deduplicated); used for ordering only. */
  selectedTopics: string[]
  /** Preferred sources echoed from the request; used for feed chips. */
  selectedSources: SourceFilterItem[]
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

export type UserProfileResponse = {
  id: number
  email: string
  name: string
  roles: string[]
}

export type AuthResponse = {
  accessToken: string
  expiresAtUtc: string
  user: UserProfileResponse
}

export type UserFeedPreferencesResponse = {
  topicNames: string[]
  sourceIds: number[]
  minPositivity: number
  sortBy: string
}
