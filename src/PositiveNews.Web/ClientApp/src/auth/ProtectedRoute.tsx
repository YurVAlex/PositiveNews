import type { ReactElement } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from './AuthProvider'

type ProtectedRouteProps = {
  element: ReactElement
  roles?: string[]
}

export function ProtectedRoute({ element, roles = [] }: ProtectedRouteProps) {
  const { isLoading, isAuthenticated, user } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return <div className="alert alert-secondary mt-3 mb-0">Checking authentication...</div>
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (roles.length > 0) {
    const userRoles = new Set((user?.roles ?? []).map((r) => r.toLowerCase()))
    const hasRole = roles.some((role) => userRoles.has(role.toLowerCase()))
    if (!hasRole) {
      return <div className="alert alert-warning mt-3 mb-0">You do not have permission to view this page.</div>
    }
  }

  return element
}
