/** Shared HTTP helpers for building API URLs and auth headers used by all client API modules. */

const apiBase = (import.meta.env.VITE_API_BASE ?? '').replace(/\/$/, '')

/** Resolves a path against VITE_API_BASE so callers can use relative `/api/...` paths. */
export function apiUrl(path: string) {
  return `${apiBase}${path.startsWith('/') ? '' : '/'}${path}`
}

/** Returns fetch headers with Bearer auth when a token exists; anonymous calls still request JSON. */
export function authTokenHeader(token: string | null): HeadersInit {
  if (!token) {
    return { Accept: 'application/json' }
  }

  return {
    Accept: 'application/json',
    Authorization: `Bearer ${token}`,
  }
}
