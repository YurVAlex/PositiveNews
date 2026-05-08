import { Link } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useAuth } from '../auth/AuthProvider'

export function AppLayout({ children }: { children: ReactNode }) {
  const { isAuthenticated, user, logout } = useAuth()

  return (
    <>
      <header>
        <nav className="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-1">
          <div className="container-fluid">
            <Link className="navbar-brand" to="/">
              PositiveNews.Web
            </Link>
            <button
              className="navbar-toggler"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target=".navbar-collapse"
              aria-controls="navbarSupportedContent"
              aria-expanded="false"
              aria-label="Toggle navigation"
            >
              <span className="navbar-toggler-icon" />
            </button>
            <div className="navbar-collapse collapse d-sm-inline-flex justify-content-between">
              <ul className="navbar-nav flex-grow-1">
                <li className="nav-item">
                  <Link className="nav-link text-dark" to="/">
                    Home
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link text-dark" to="/privacy">
                    Privacy
                  </Link>
                </li>
              </ul>
              <div className="d-flex align-items-center gap-2 mt-2 mt-sm-0">
                {isAuthenticated ? (
                  <>
                    <span className="text-muted fs-6">Hello, {user?.name ?? 'User'}</span>
                    <button type="button" className="btn btn-sm btn-outline-secondary" onClick={logout}>
                      Logout
                    </button>
                  </>
                ) : (
                  <Link className="btn btn-sm btn-primary" to="/login">
                    Login
                  </Link>
                )}
              </div>
            </div>
          </div>
        </nav>
      </header>

      <div className="container">{children}</div>

      <footer className="border-top footer text-muted">
        <div className="container">
          &copy; 2026 - PositiveNews.Web -{' '}
          <Link to="/privacy" className="text-muted">
            Privacy
          </Link>
        </div>
      </footer>
    </>
  )
}
