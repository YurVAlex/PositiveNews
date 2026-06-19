const apiBase = (import.meta.env.VITE_API_BASE ?? '').replace(/\/$/, '')

export function apiUrl(path: string) {
  return `${apiBase}${path.startsWith('/') ? '' : '/'}${path}`
}

export function authTokenHeader(token: string | null): HeadersInit {
  if (!token) {
    return { Accept: 'application/json' }
  }

  return {
    Accept: 'application/json',
    Authorization: `Bearer ${token}`,
  }
}
