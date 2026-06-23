/** Topic metadata for feed filter dropdowns and preference pickers. */

import type { TopicsMetadataResponse } from './types'
import { apiUrl, authTokenHeader } from './http'

/** Loads available topics; optional token may unlock personalized ordering in the future. */
export async function fetchTopics(token: string | null = null): Promise<TopicsMetadataResponse> {
  const res = await fetch(apiUrl('/api/topics'), {
    headers: authTokenHeader(token),
  })

  if (!res.ok) {
    throw new Error(`Topics request failed (${res.status})`)
  }

  return res.json() as Promise<TopicsMetadataResponse>
}
