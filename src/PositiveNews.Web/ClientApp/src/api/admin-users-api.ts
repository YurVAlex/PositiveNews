import { apiUrl, authTokenHeader } from './http'

export type AdminUserItem = {
  id: number
  name: string
  isActive: boolean
  emailConfirmed: boolean
  failedLoginCount: number
  createdAt: string
  moderatedBy: number | null
}

export type AdminUserDetail = {
  id: number
  name: string
  email: string
  isActive: boolean
  emailConfirmed: boolean
  failedLoginCount: number
  createdAt: string
  lastLoginAt: string | null
  moderatedBy: number | null
}

export type UpdateUserRequest = {
  isActive: boolean
  emailConfirmed: boolean
  reason?: string | null
  note?: string | null
}

async function parseProblem(res: Response): Promise<string> {
  try {
    const body = (await res.json()) as { detail?: string; title?: string }
    return body.detail ?? body.title ?? `Request failed (${res.status})`
  } catch {
    return `Request failed (${res.status})`
  }
}

export async function fetchAdminUsers(token: string, searchTerm?: string): Promise<AdminUserItem[]> {
  const uri = searchTerm ? apiUrl(`/api/admin/users?q=${encodeURIComponent(searchTerm)}`) : apiUrl('/api/admin/users')
  const res = await fetch(uri, { headers: authTokenHeader(token) })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<AdminUserItem[]>
}

export async function fetchAdminUserDetail(token: string, userId: number): Promise<AdminUserDetail> {
  const res = await fetch(apiUrl(`/api/admin/users/${userId}`), { headers: authTokenHeader(token) })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('User not found')
  if (!res.ok) throw new Error(await parseProblem(res))

  return res.json() as Promise<AdminUserDetail>
}

export async function updateAdminUser(token: string, userId: number, payload: UpdateUserRequest): Promise<void> {
  const res = await fetch(apiUrl(`/api/admin/users/${userId}`), {
    method: 'PUT',
    headers: {
      ...authTokenHeader(token),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (res.status === 401) throw new Error('Unauthorized')
  if (res.status === 403) throw new Error('Forbidden')
  if (res.status === 404) throw new Error('User not found')
  if (!res.ok) throw new Error(await parseProblem(res))
}