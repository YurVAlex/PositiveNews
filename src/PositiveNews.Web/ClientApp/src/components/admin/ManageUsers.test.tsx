import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ManageUsers } from './ManageUsers'

const mockUsers = [
  {
    id: 1,
    name: 'Jane Doe',
    isActive: true,
    emailConfirmed: false,
    failedLoginCount: 2,
    createdAt: '2026-05-22T10:00:00Z',
    moderatedBy: null,
  },
]

const mockUserDetail = {
  id: 1,
  name: 'Jane Doe',
  email: 'jane@example.com',
  isActive: true,
  emailConfirmed: false,
  failedLoginCount: 2,
  createdAt: '2026-05-22T10:00:00Z',
  lastLoginAt: '2026-05-23T10:00:00Z',
  moderatedBy: null,
}

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ token: 'test-token', user: null, isAuthenticated: true }),
}))

vi.mock('../../api/admin-users-api', () => ({
  fetchAdminUsers: vi.fn(),
  fetchAdminUserDetail: vi.fn(),
  updateAdminUser: vi.fn(),
}))

describe('ManageUsers', () => {
  let api: {
    fetchAdminUsers: ReturnType<typeof vi.fn>
    fetchAdminUserDetail: ReturnType<typeof vi.fn>
    updateAdminUser: ReturnType<typeof vi.fn>
  }

  beforeEach(async () => {
    vi.clearAllMocks()
    api = await vi.importMock('../../api/admin-users-api')
    api.fetchAdminUsers.mockResolvedValue(mockUsers)
    api.fetchAdminUserDetail.mockResolvedValue(mockUserDetail)
    api.updateAdminUser.mockResolvedValue(undefined)
  })

  it('shows a searchable user table and renders moderated column', async () => {
    const user = userEvent.setup()

    render(<ManageUsers />)

    await user.click(screen.getByRole('button', { name: 'Search' }))

    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    expect(screen.getByRole('columnheader', { name: 'Moderated' })).toBeInTheDocument()
    expect(screen.getByText('Jane Doe')).toBeInTheDocument()
    expect(api.fetchAdminUsers).toHaveBeenCalledWith('test-token', '')
  })

  it('opens details and submits user moderation changes', async () => {
    const user = userEvent.setup()

    render(<ManageUsers />)

    await user.click(screen.getByRole('button', { name: 'Search' }))
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByText('Jane Doe'))
    await waitFor(() => expect(screen.getByDisplayValue('jane@example.com')).toBeInTheDocument())

    await user.click(screen.getByLabelText('Email confirmed'))
    await user.click(screen.getByRole('button', { name: 'Apply' }))

    await waitFor(() => expect(api.updateAdminUser).toHaveBeenCalled())
    expect(api.updateAdminUser).toHaveBeenCalledWith('test-token', 1, expect.objectContaining({ emailConfirmed: true }))
    expect(screen.getByRole('status')).toHaveTextContent('User updated successfully.')
  })

  it('clears search results and selection when Clear is clicked', async () => {
    const user = userEvent.setup()

    render(<ManageUsers />)

    await user.click(screen.getByRole('button', { name: 'Search' }))
    await waitFor(() => expect(screen.getByRole('table')).toBeInTheDocument())
    await user.click(screen.getByRole('button', { name: 'Clear' }))

    await waitFor(() => expect(screen.queryByRole('table')).not.toBeInTheDocument())
  })
})