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
  moderatedBy: null,
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

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ token: 'test-token', user: null, isAuthenticated: true }),
}))

vi.mock('../../api/admin-comments-api', () => ({
  fetchAdminCommentDetail: vi.fn(),
  updateAdminComment: vi.fn(),
}))

describe('ManageComments', () => {
  let api: {
    fetchAdminCommentDetail: ReturnType<typeof vi.fn>
    updateAdminComment: ReturnType<typeof vi.fn>
  }

  beforeEach(async () => {
    vi.clearAllMocks()
    api = await vi.importMock('../../api/admin-comments-api')
    api.fetchAdminCommentDetail.mockResolvedValue(mockCommentDetail)
    api.updateAdminComment.mockResolvedValue(undefined)
  })

  it('loads comment detail when searching with a valid id', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await user.type(screen.getByPlaceholderText('Comment id'), '42')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(screen.getByDisplayValue('A thoughtful comment')).toBeInTheDocument())
    expect(screen.getByDisplayValue('Jane Doe')).toBeInTheDocument()
    expect(screen.getByText('Bob Smith')).toBeInTheDocument()
    expect(api.fetchAdminCommentDetail).toHaveBeenCalledWith('test-token', 42)
  })

  it('submits moderation changes via Apply', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await user.type(screen.getByPlaceholderText('Comment id'), '42')
    await user.click(screen.getByRole('button', { name: 'Search' }))
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

  it('shows validation message for invalid id without calling API', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await user.type(screen.getByPlaceholderText('Comment id'), 'abc')
    await user.click(screen.getByRole('button', { name: 'Search' }))

    expect(screen.getByRole('alert')).toHaveTextContent('Enter a valid positive comment id.')
    expect(api.fetchAdminCommentDetail).not.toHaveBeenCalled()
  })

  it('hides details panel when Close is clicked', async () => {
    const user = userEvent.setup()

    render(<ManageComments />)

    await user.type(screen.getByPlaceholderText('Comment id'), '42')
    await user.click(screen.getByRole('button', { name: 'Search' }))
    await waitFor(() => expect(screen.getByDisplayValue('A thoughtful comment')).toBeInTheDocument())

    await user.click(screen.getByRole('button', { name: 'Close' }))

    await waitFor(() => expect(screen.queryByDisplayValue('A thoughtful comment')).not.toBeInTheDocument())
    expect(screen.getByText('Search for a comment to review and update its state.')).toBeInTheDocument()
  })
})
