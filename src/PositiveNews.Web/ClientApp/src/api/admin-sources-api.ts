/** Admin RSS source configuration and trust-score management. */

import { apiUrl, authTokenHeader } from './http'

export type SourceAdminItem = {
  id: number
  name: string
  trustScore: number
  isActive: boolean
  moderatedBy: number | null
}

export type SourceAdminDetail = {
  id: number
  name: string
  trustScore: number
  isActive: boolean
  feedUrl: string
  moderatedBy: number | null
}

export type UpdateSourceRequest = {
  trustScore: number
  isActive: boolean
  feedUrl: string
  reason?: string | null
  note?: string | null
}

// Extract a human-readable message from ASP.NET ProblemDetails responses.
async function parseProblem(res: Response): Promise<string> {
  try {
    const body = (await res.json()) as { detail?: string; title?: string }
    return body.detail ?? body.title ?? `Request failed (${res.status})`
  } catch {
    return `Request failed (${res.status})`
  }
}

/** Lists all ingestion sources with summary fields for the admin table. */
export async function fetchAdminSources(token: string): Promise<SourceAdminItem[]> {
  const res = await fetch(apiUrl('/api/admin/sources'), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<SourceAdminItem[]>
}

/** Loads feed URL and moderation metadata for editing a single source. */
export async function fetchSourceDetail(token: string, sourceId: number): Promise<SourceAdminDetail> {
  const res = await fetch(apiUrl(`/api/admin/sources/${sourceId}`), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('Source not found')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<SourceAdminDetail>
}

/** Updates trust score, feed URL, or active flag; changes affect future ingestion runs. */
export async function updateSource(token: string, sourceId: number, payload: UpdateSourceRequest): Promise<void> {
  const res = await fetch(apiUrl(`/api/admin/sources/${sourceId}`), {
    method: 'PUT',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('Source not found')
  if (!res.ok) {
    throw new Error(await parseProblem(res))
  }
}
