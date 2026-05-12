import { apiUrl, authTokenHeader } from './http'

describe('apiUrl', () => {
  it('keeps relative API paths when no base URL is configured', () => {
    expect(apiUrl('/api/articles')).toBe('/api/articles')
    expect(apiUrl('api/articles')).toBe('/api/articles')
  })
})

describe('authTokenHeader', () => {
  it('returns accept header without authorization when token is missing', () => {
    expect(authTokenHeader(null)).toEqual({ Accept: 'application/json' })
  })

  it('returns bearer authorization when token is present', () => {
    expect(authTokenHeader('token-123')).toEqual({
      Accept: 'application/json',
      Authorization: 'Bearer token-123',
    })
  })
})
