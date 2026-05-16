import type { SourcesMetadataResponse } from './types'
import { apiUrl, authTokenHeader } from './http'

export async function fetchSources(token: string | null = null): Promise<SourcesMetadataResponse> {
  const res = await fetch(apiUrl('/api/sources'), {
    headers: authTokenHeader(token),
  })

  if (!res.ok) {
    throw new Error(`Sources request failed (${res.status})`)
  }

  return res.json() as Promise<SourcesMetadataResponse>
}
