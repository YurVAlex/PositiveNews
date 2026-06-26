/**
 * Route guard: waits for auth bootstrap, redirects guests to login, and optionally checks roles.
 */
import type { ReactElement } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from './AuthProvider'

type ProtectedRouteProps = {
  element: ReactElement
  roles?: string[]
}

/** Renders the page element only when the user is authenticated (and has a required role, if any). */
export function ProtectedRoute({ element, roles = [] }: ProtectedRouteProps) {
  const { isLoading, isAuthenticated, user } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return <div className="alert alert-secondary mt-3 mb-0">Checking authentication...</div>
  }

  // Preserve attempted path so LoginPage can send the user back after sign-in.
  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  // Case-insensitive role check matches server role names regardless of casing.
  if (roles.length > 0) {
    const userRoles = new Set((user?.roles ?? []).map((r) => r.toLowerCase()))
    const hasRole = roles.some((role) => userRoles.has(role.toLowerCase()))
    if (!hasRole) {
      return <div className="alert alert-warning mt-3 mb-0">You do not have permission to view this page.</div>
    }
  }

  return element
}
