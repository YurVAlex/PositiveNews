import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ManageComments } from './ManageComments'

const mockCommentDetail = {
  id: 42,
  content: 'A thoughtful comment',
  createdAt: '2026-05-22T10:00:00Z',
  userId: 2,
  userName: 'Jane Doe',
  isActive: true,
  moderatedBy: 7,
  articleId: 1,
  complaints: [
    {
      id: 1,
      userId: 3,
      userName: 'Bob Smith',
      reason: 'Spam',
      createdAt: '2026-05-23T10:00:00Z',
    },
  ],
}

const mockComments = [
  {
    id: 42,
    articleId: 1,
    userId: 2,
    complaintCount: 2,
    isActive: true,
    moderatedBy: 7,
  },
]

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ token: 'test-token', user: null, isAuthenticated: true }),
}))

vi.mock('../../api/admin-comments-api', () => ({
  fetchAdminCommentDetail: vi.fn(),
  fetchAdminComments: vi.fn(),
  updateAdminComment: vi.fn(),
}))

describe('ManageComments', () => {
  let api: {
    fetchAdminCommentDetail: ReturnType<typeof vi.fn>
    fetchAdminComments: ReturnType<typeof vi.fn>
    updateAdminComment: ReturnType<typeof vi.fn>
  }

  beforeEach(async () => {
    vi.clearAllMocks()
    api = await vi.importMock('../../api/admin-comments-api')
    api.fetchAdminCommentDetail.mockResolvedValue(mockCommentDetail)
    api.fetchAdminComments.mockResolvedValue(mockComments)
    api.updateAdminComment.mockResolvedValue(undefined)
  })

  it('loads active comments table on mount', async () => {
    render(<ManageComments />)

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    expect(screen.getByRole('columnheader', { name: 'ModeratedBy' })).toBeInTheDocument()
    expect(screen.queryByRole('columnheader', { name: 'IsActive' })).not.toBeInTheDocument()
    expect(api.fetchAdminComments).toHaveBeenCalledWith('test-token')
  })

  it('loads comment detail when searching with a valid id', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.type(screen.getByPlaceholderText('Comment id'), '42')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(screen.getByDisplayValue('A thoughtful comment')).toBeInTheDocument())
    expect(api.fetchAdminCommentDetail).toHaveBeenCalledWith('test-token', 42)
  })

  it('loads detail when a table row is clicked', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByText('42'))

    await waitFor(() => expect(screen.getByDisplayValue('A thoughtful comment')).toBeInTheDocument())
  })

  it('submits moderation changes via Apply', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByText('42'))
    await waitFor(() => expect(screen.getByDisplayValue('A thoughtful comment')).toBeInTheDocument())

    await user.click(screen.getByLabelText('Is active'))
    await user.type(screen.getByLabelText('Reason'), 'policy')
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    await waitFor(() => expect(api.updateAdminComment).toHaveBeenCalled())
    expect(api.updateAdminComment).toHaveBeenCalledWith('test-token', 42, expect.objectContaining({
      isActive: false,
      reason: 'policy',
    }))
    expect(screen.getByRole('status')).toHaveTextContent('Comment updated successfully.')
  })

  it('shows validation message for invalid id without calling detail API', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await user.type(screen.getByPlaceholderText('Comment id'), 'abc')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(screen.getByRole('alert')).toHaveTextContent('Enter a valid positive comment id.')
    expect(api.fetchAdminCommentDetail).not.toHaveBeenCalled()
  })
})
