import { apiUrl, authTokenHeader } from './http'
import type { AuthResponse, UserProfileResponse } from './types'

type ApiProblemDetails = {
  title?: string
  detail?: string
}

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
