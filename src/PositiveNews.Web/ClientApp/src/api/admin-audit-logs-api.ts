import { apiUrl, authTokenHeader } from './http'

export type AuditLogAdminItem = {
  id: number
  entityType: string
  entityId: number
  changedField?: string | null
  oldValue?: string | null
  newValue?: string | null
  moderatorId: number
  createdAt: string
  reason?: string | null
  note?: string | null
}

export async function fetchAuditLogs(token: string, limit = 100): Promise<AuditLogAdminItem[]> {
  const res = await fetch(apiUrl(`/api/admin/audit-logs?limit=${limit}`), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(`Request failed (${res.status})`)

  return res.json() as Promise<AuditLogAdminItem[]>
}
