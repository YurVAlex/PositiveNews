import { fetchSources } from './sources-api'

const fetchMock = vi.fn()

beforeEach(() => {
  fetchMock.mockReset()
  vi.stubGlobal('fetch', fetchMock)
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('fetchSources', () => {
  it('requests catalog sources with auth header', async () => {
    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({
          sources: [{ id: 1, name: 'Source A', logoUrl: null }],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await fetchSources('token')

    expect(result.sources).toHaveLength(1)
    expect(fetchMock).toHaveBeenCalledWith('/api/sources', {
      headers: {
        Accept: 'application/json',
        Authorization: 'Bearer token',
      },
    })
  })

  it('throws when the sources request fails', async () => {
    fetchMock.mockResolvedValue(new Response(null, { status: 500 }))

    await expect(fetchSources()).rejects.toThrow('Sources request failed (500)')
  })
})
