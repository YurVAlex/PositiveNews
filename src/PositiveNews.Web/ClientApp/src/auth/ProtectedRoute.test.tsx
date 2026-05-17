import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './ProtectedRoute'
import type { UserProfileResponse } from '../api/types'

type MockAuthState = {
  isLoading: boolean
  isAuthenticated: boolean
  user: UserProfileResponse | null
  token: string | null
  login: ReturnType<typeof vi.fn>
  register: ReturnType<typeof vi.fn>
  logout: ReturnType<typeof vi.fn>
}

const authState = vi.hoisted(() => ({
  value: {
    isLoading: false,
    isAuthenticated: true,
    user: { id: 1, email: 'admin@example.com', name: 'Admin', roles: ['Admin'] },
    token: 'token',
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
  } as MockAuthState,
}))

vi.mock('./AuthProvider', () => ({
  useAuth: () => authState.value,
}))

describe('ProtectedRoute', () => {
  beforeEach(() => {
    authState.value = {
      isLoading: false,
      isAuthenticated: true,
      user: { id: 1, email: 'admin@example.com', name: 'Admin', roles: ['Admin'] },
      token: 'token',
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    }
  })

  it('shows loading message while auth is resolving', () => {
    authState.value = { ...authState.value, isLoading: true, isAuthenticated: false, user: null, token: null }

    renderProtectedRoute()

    expect(screen.getByText('Checking authentication...')).toBeInTheDocument()
  })

  it('redirects anonymous users to login', () => {
    authState.value = { ...authState.value, isAuthenticated: false, user: null, token: null }

    renderProtectedRoute()

    expect(screen.getByText('Login page')).toBeInTheDocument()
  })

  it('denies authenticated users without the required role case-insensitively', () => {
    authState.value = {
      ...authState.value,
      user: { id: 2, email: 'user@example.com', name: 'User', roles: ['user'] },
    }

    renderProtectedRoute()

    expect(screen.getByText('You do not have permission to view this page.')).toBeInTheDocument()
  })

  it('renders protected content when required role is present', () => {
    authState.value = {
      ...authState.value,
      user: { id: 1, email: 'admin@example.com', name: 'Admin', roles: ['admin'] },
    }

    renderProtectedRoute()

    expect(screen.getByText('Admin content')).toBeInTheDocument()
  })
})

function renderProtectedRoute() {
  render(
    <MemoryRouter initialEntries={['/admin']}>
      <Routes>
        <Route
          path="/admin"
          element={<ProtectedRoute roles={['Admin']} element={<div>Admin content</div>} />}
        />
        <Route path="/login" element={<div>Login page</div>} />
      </Routes>
    </MemoryRouter>,
  )
}
