/** Admin RSS ingestion monitoring and manual cycle trigger. */

import { apiUrl, authTokenHeader } from './http'

export type IngestionCycleStatus = {
  isRunning: boolean
  nextRunAtUtc: string | null
}

export type IngestionRunListItem = {
  id: number
  sourceName: string
  startedAt: string
  finishedAt: string | null
  status: string
  itemsFetched: number
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

/** Reports whether a cycle is in progress and when the scheduler will run next. */
export async function fetchIngestionStatus(token: string): Promise<IngestionCycleStatus> {
  const res = await fetch(apiUrl('/api/admin/ingestion/status'), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<IngestionCycleStatus>
}

/** Returns recent per-source ingestion runs for the admin activity log. */
export async function fetchIngestionRuns(token: string): Promise<IngestionRunListItem[]> {
  const res = await fetch(apiUrl('/api/admin/ingestion/runs'), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<IngestionRunListItem[]>
}

/** Starts an on-demand ingestion cycle; no-ops on 202, rejects if a cycle is already running (409). */
export async function triggerIngestionCycle(token: string): Promise<void> {
  const res = await fetch(apiUrl('/api/admin/ingestion/trigger'), {
    method: 'POST',
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 409) throw new Error('An ingestion cycle is already in progress.')
  if (res.status === 202) return
  if (!res.ok) throw new Error(await parseProblem(res))
}
