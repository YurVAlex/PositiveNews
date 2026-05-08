import { apiUrl, authTokenHeader } from './http'
import type { AuthResponse, UserProfileResponse } from './types'

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
      throw new Error('A user with this email already exists.')
    }
    if (res.status === 400) {
      throw new Error('Please fill in all required fields.')
    }
    throw new Error(`Registration failed (${res.status})`)
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
    throw new Error(res.status === 401 ? 'Invalid email or password.' : `Login failed (${res.status})`)
  }

  return res.json() as Promise<AuthResponse>
}

export async function getCurrentUser(token: string): Promise<UserProfileResponse> {
  const res = await fetch(apiUrl('/api/auth/me'), {
    headers: authTokenHeader(token),
  })

  if (res.status === 401) {
    throw new Error('Unauthorized')
  }
  if (!res.ok) {
    throw new Error(`Current user request failed (${res.status})`)
  }

  return res.json() as Promise<UserProfileResponse>
}
