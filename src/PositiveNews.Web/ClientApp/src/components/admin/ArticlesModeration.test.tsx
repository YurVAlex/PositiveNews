import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ArticlesModeration } from './ArticlesModeration'

const mockArticles = [
  {
    id: 1,
    sourceId: 10,
    sourceName: 'Source A',
    title: 'Article One',
    positivityScore: 0.42,
    isActive: true,
    moderatedBy: null,
    publishedAt: '2026-05-22T10:00:00Z',
  },
]

const mockArticleDetail = {
  id: 1,
  sourceId: 10,
  sourceName: 'Source A',
  title: 'Article One',
  positivityScore: 0.42,
  isActive: true,
  moderatedBy: null,
  publishedAt: '2026-05-22T10:00:00Z',
  sourceLogoUrl: 'https://example.com/logo.png',
  author: 'Jane Doe',
  url: 'https://example.com/article-one',
  summaryShort: 'A brief summary.',
  imageTag: '<img src="cover.jpg" />',
  contentRaw: '<p>Raw content</p>',
}

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ token: 'test-token', user: null, isAuthenticated: true }),
}))

vi.mock('../../api/admin-articles-api', () => ({
  fetchAdminArticles: vi.fn(),
  fetchAdminArticleDetail: vi.fn(),
  moderateArticle: vi.fn(),
}))

describe('ArticlesModeration', () => {
  let api: {
    fetchAdminArticles: ReturnType<typeof vi.fn>
    fetchAdminArticleDetail: ReturnType<typeof vi.fn>
    moderateArticle: ReturnType<typeof vi.fn>
  }

  beforeEach(async () => {
    vi.clearAllMocks()
    api = await vi.importMock('../../api/admin-articles-api')
    api.fetchAdminArticles.mockResolvedValue(mockArticles)
    api.fetchAdminArticleDetail.mockResolvedValue(mockArticleDetail)
    api.moderateArticle.mockResolvedValue(undefined)
  })

  it('shows search UI and loads article table after search', async () => {
    const user = userEvent.setup()

    render(<ArticlesModeration />)

    expect(screen.queryByRole('table')).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Search' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Clear' })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    expect(screen.getByRole('columnheader', { name: 'Positivity' })).toBeInTheDocument()
    expect(screen.getByRole('columnheader', { name: 'Active' })).toBeInTheDocument()
    expect(api.fetchAdminArticles).toHaveBeenCalledWith('test-token', '')
  })

  it('shows moderator id in the ModeratedBy column when the article has been moderated', async () => {
    const user = userEvent.setup()
    api.fetchAdminArticles.mockResolvedValue([
      {
        ...mockArticles[0],
        moderatedBy: 42,
      },
    ])

    render(<ArticlesModeration />)

    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())

    const row = screen.getByText('Article One').closest('tr')
    expect(row).not.toBeNull()
    expect(within(row!).getAllByRole('cell')[5]).toHaveTextContent('42')
  })

  it('clears search results and selection when Clear is clicked', async () => {
    const user = userEvent.setup()

    render(<ArticlesModeration />)

    await user.click(screen.getByRole('button', { name: 'Search' }))
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByText('Article One'))
    await waitFor(() => expect(screen.getByLabelText('Title')).toBeInTheDocument())

    await user.click(screen.getByRole('button', { name: 'Clear' }))

    await waitFor(() => expect(screen.queryByRole('table')).not.toBeInTheDocument())
    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument()
  })

  it('cancels selection and resets the moderation details section', async () => {
    const user = userEvent.setup()

    render(<ArticlesModeration />)

    await user.click(screen.getByRole('button', { name: 'Search' }))
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByText('Article One'))
    await waitFor(() => expect(screen.getByLabelText('Title')).toBeInTheDocument())

    await user.click(screen.getByRole('button', { name: 'Cancel' }))

    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument()
  })

  it('closes details and skips detail refetch after Apply', async () => {
    const user = userEvent.setup()

    render(<ArticlesModeration />)

    await user.click(screen.getByRole('button', { name: 'Search' }))
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByText('Article One'))
    await waitFor(() => expect(screen.getByLabelText('Title')).toBeInTheDocument())

    const detailCallsBeforeApply = api.fetchAdminArticleDetail.mock.calls.length

    await user.click(screen.getByRole('button', { name: 'Apply' }))

    await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('Article moderation saved successfully.'))
    expect(screen.queryByLabelText('Title')).not.toBeInTheDocument()
    expect(api.fetchAdminArticles).toHaveBeenCalledTimes(2)
    expect(api.fetchAdminArticleDetail.mock.calls.length).toBe(detailCallsBeforeApply)
  })
})
