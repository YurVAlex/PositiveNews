import { deactivateAccount, getCurrentUser, login, register } from './auth-api'
import type { AuthResponse, UserProfileResponse } from './types'

const fetchMock = vi.fn()

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('register', () => {
  it('posts registration data and returns auth response', async () => {
    const auth = authResponse()
    fetchMock.mockResolvedValue(jsonResponse(auth))

    await expect(register('user@example.com', 'Jane', 'Password1!')).resolves.toEqual(auth)

    expect(fetchMock).toHaveBeenCalledWith('/api/auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify({ email: 'user@example.com', name: 'Jane', password: 'Password1!' }),
    })
  })

  it('uses problem details for conflict errors', async () => {
    fetchMock.mockResolvedValue(jsonResponse({ detail: 'Email already exists.' }, 409))

    await expect(register('user@example.com', 'Jane', 'Password1!')).rejects.toThrow('Email already exists.')
  })
})

describe('login', () => {
  it('uses title fallback for validation errors', async () => {
    fetchMock.mockResolvedValue(jsonResponse({ title: 'Bad login data' }, 400))

    await expect(login('bad', '')).rejects.toThrow('Bad login data')
  })

  it('uses default message when error body cannot be parsed', async () => {
    fetchMock.mockResolvedValue(new Response('not-json', { status: 401 }))

    await expect(login('user@example.com', 'wrong')).rejects.toThrow('Invalid email or password.')
  })
})

describe('deactivateAccount', () => {
  it('sends delete request with bearer token', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 200 }))

    await deactivateAccount('token')

    expect(fetchMock).toHaveBeenCalledWith('/api/auth/me', {
      method: 'DELETE',
      headers: {
        Accept: 'application/json',
        Authorization: 'Bearer token',
      },
    })
  })
})

describe('getCurrentUser', () => {
  it('sends bearer token and returns current user', async () => {
    const user: UserProfileResponse = { id: 1, email: 'user@example.com', name: 'Jane', roles: ['Admin'] }
    fetchMock.mockResolvedValue(jsonResponse(user))

    await expect(getCurrentUser('token')).resolves.toEqual(user)

    expect(fetchMock).toHaveBeenCalledWith('/api/auth/me', {
      headers: {
        Accept: 'application/json',
        Authorization: 'Bearer token',
      },
    })
  })
})

function authResponse(): AuthResponse {
  return {
    accessToken: 'token',
    expiresAtUtc: '2026-05-11T00:00:00Z',
    refreshToken: 'refresh-token',
    user: { id: 1, email: 'user@example.com', name: 'Jane', roles: ['User'] },
  }
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}
