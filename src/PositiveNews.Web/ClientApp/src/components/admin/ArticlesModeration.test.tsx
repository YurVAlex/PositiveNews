import { render, screen, waitFor } from '@testing-library/react'
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
})
