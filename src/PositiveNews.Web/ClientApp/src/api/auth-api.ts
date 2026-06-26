/** Authentication and account lifecycle API calls (register, login, session refresh, deactivation). */

import { apiUrl, authTokenHeader } from './http'
import type { AuthResponse, UserProfileResponse } from './types'

type ApiProblemDetails = {
  title?: string
  detail?: string
}

// Prefer server ProblemDetails detail/title over generic status-based fallbacks.
async function readErrorMessage(res: Response, fallback: string): Promise<string> {
  try {
    const data = (await res.json()) as ApiProblemDetails
    if (typeof data.detail === 'string' && data.detail.trim().length > 0) {
      return data.detail
    }
    if (typeof data.title === 'string' && data.title.trim().length > 0) {
      return data.title
    }
  } catch {
    // Ignore parse errors and use fallback message.
  }

  return fallback
}

/** Creates a new account and returns tokens for immediate sign-in. */
export async function register(email: string, name: string, password: string): Promise<AuthResponse> {
  const res = await fetch(apiUrl('/api/auth/register'), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify({ email, name, password }),
  })

  if (!res.ok) {
    if (res.status === 409) {
      throw new Error(await readErrorMessage(res, 'A user with this email already exists.'))
    }
    if (res.status === 400) {
      throw new Error(await readErrorMessage(res, 'Registration data is invalid.'))
    }
    throw new Error(await readErrorMessage(res, `Registration failed (${res.status})`))
  }

  return res.json() as Promise<AuthResponse>
}

/** Authenticates credentials and returns access/refresh tokens. */
export async function login(email: string, password: string): Promise<AuthResponse> {
  const res = await fetch(apiUrl('/api/auth/login'), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify({ email, password }),
  })

  if (!res.ok) {
    if (res.status === 401) {
      throw new Error(await readErrorMessage(res, 'Invalid email or password.'))
    }
    if (res.status === 400) {
      throw new Error(await readErrorMessage(res, 'Login data is invalid.'))
    }
    throw new Error(await readErrorMessage(res, `Login failed (${res.status})`))
  }

  return res.json() as Promise<AuthResponse>
}

/** Loads the signed-in user's profile; used to validate tokens and hydrate auth state. */
export async function getCurrentUser(token: string): Promise<UserProfileResponse> {
  const res = await fetch(apiUrl('/api/auth/me'), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) {
    throw new Error(await readErrorMessage(res, 'Unauthorized'))
  }
  if (!res.ok) {
    throw new Error(await readErrorMessage(res, `Current user request failed (${res.status})`))
  }

  return res.json() as Promise<UserProfileResponse>
}

/** Soft-deletes the current account so the user cannot sign in again. */
export async function deactivateAccount(token: string): Promise<void> {
  const res = await fetch(apiUrl('/api/auth/me'), {
    method: 'DELETE',
    headers: authTokenHeader(token),
  })

  if (res.status === 401) {
    throw new Error(await readErrorMessage(res, 'Unauthorized'))
  }
  if (res.status === 409) {
    throw new Error(await readErrorMessage(res, 'Account is already deactivated.'))
  }
  if (!res.ok) {
    throw new Error(await readErrorMessage(res, `Account deactivation failed (${res.status})`))
  }
}

/** Exchanges a refresh token for a new access token without re-entering credentials. */
export async function refreshToken(token: string): Promise<AuthResponse> {
  const res = await fetch(apiUrl('/api/auth/refresh'), {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify({ refreshToken: token }),
  })

  if (!res.ok) {
    if (res.status === 401) {
      throw new Error(await readErrorMessage(res, 'Invalid or expired refresh token.'))
    }
    throw new Error(await readErrorMessage(res, `Token refresh failed (${res.status})`))
  }

  return res.json() as Promise<AuthResponse>
}
