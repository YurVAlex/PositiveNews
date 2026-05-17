import { apiUrl, authTokenHeader } from './http'

type AdminStatusResponse = {
  ok: boolean
  message: string
}

export async function fetchAdminStatus(token: string): Promise<AdminStatusResponse> {
  const res = await fetch(apiUrl('/api/admin/status'), {
    headers: authTokenHeader(token),
  })

  if (res.status === 403) {
    throw new Error('Forbidden')
  }
  if (res.status === 401) {
    throw new Error('Unauthorized')
  }
  if (!res.ok) {
    throw new Error(`Admin status request failed (${res.status})`)
  }

  return res.json() as Promise<AdminStatusResponse>
}
