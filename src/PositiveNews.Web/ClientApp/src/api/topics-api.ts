import type { TopicsMetadataResponse } from './types'
import { apiUrl, authTokenHeader } from './http'

export async function fetchTopics(token: string | null = null): Promise<TopicsMetadataResponse> {
  const res = await fetch(apiUrl('/api/topics'), {
    headers: authTokenHeader(token),
  })

  if (!res.ok) {
    throw new Error(`Topics request failed (${res.status})`)
  }

  return res.json() as Promise<TopicsMetadataResponse>
}
