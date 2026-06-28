/** App shell: navbar, footer, and feed-aware navigation around routed page content. */
import { Link, useLocation, useNavigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { useMemo } from 'react'
import { useAuth } from '../auth/AuthProvider'
import loginIcon from '../assets/ui/login.svg'
import logoutIcon from '../assets/ui/logout.svg'
import settingsIcon from '../assets/ui/settings.svg'
import {
  buildFeedReturnTo,
  buildSearchFromSnapshot,
  DEFAULT_MIN_POSITIVITY,
  isSettingsOpen,
  loadFeedPrefsDraft,
} from '../utils/feed-preferences-url'

type FeedNavigationState = {
  feedSearch?: string
}

const EMPTY_FEED_PREFS = {
  topics: [],
  sourceIds: [],
  sort: 'date' as const,
  minPositivity: DEFAULT_MIN_POSITIVITY,
}

export function AppLayout({ children }: { children: ReactNode }) {
  const { isAuthenticated, user, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()

  // Brand link should return to the feed with the user's last filters, not always bare "/".
  const feedHomeTo = useMemo(() => {
    const state = location.state as FeedNavigationState | null
    if (location.pathname === '/') {
      return buildFeedReturnTo(location.search || state?.feedSearch)
    }
    return buildFeedReturnTo(state?.feedSearch)
  }, [location.pathname, location.search, location.state])

  // Stash current feed query in link state so Privacy → back navigation can restore it.
  const feedSearchForPrivacy = useMemo(() => {
    if (location.pathname !== '/') {
      return undefined
    }
    const qs = location.search
    return qs ? qs : undefined
  }, [location.pathname, location.search])

  // On the feed, toggle ?settings=1 in place; elsewhere, jump to feed with saved prefs and settings open.
  const handleSettingsClick = () => {
    if (location.pathname === '/') {
      const params = new URLSearchParams(location.search)
      const open = isSettingsOpen(params)
      if (open) {
        params.delete('settings')
      } else {
        params.set('settings', '1')
      }
      const qs = params.toString()
      navigate(qs ? `/?${qs}` : '/', { replace: true })
      return
    }

    const draft = loadFeedPrefsDraft()
    const search = buildSearchFromSnapshot(draft ?? EMPTY_FEED_PREFS, {
      settingsOpen: true,
      page: 1,
    })
    navigate(`/${search}`, { replace: false })
  }

  // Clear feed filters from the URL on logout so the next visitor sees the default feed.
  const handleLogout = () => {
    logout()
    navigate({ pathname: '/', search: '' }, { replace: true })
  }

  const settingsActive =
    location.pathname === '/' && isSettingsOpen(new URLSearchParams(location.search))

  return (
    <>
      <header>
        <nav className="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-1">
          <div className="container-fluid">
            <Link className="navbar-brand" to={feedHomeTo}>
              Positive News
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
                  <Link
                    className="nav-link text-dark"
                    to="/privacy"
                    state={feedSearchForPrivacy ? { feedSearch: feedSearchForPrivacy } : undefined}
                  >
                    Privacy
                  </Link>
                </li>
              </ul>
              <div className="d-flex align-items-center gap-2 mt-2 mt-sm-0">
                <button
                  type="button"
                                  className={`btn btn-sm ${settingsActive ? 'btn-info' : 'btn-light'}`}
                  onClick={handleSettingsClick}
                  aria-pressed={settingsActive}
                  aria-expanded={settingsActive}
                >
                <img src={settingsIcon} alt="" width={20} height={20} className="flex-shrink-0" /> Settings
                </button>
                {isAuthenticated ? (
                  <>
                    <span className="text-muted fs-6">{user?.name ?? 'User'}</span>
                    <button
                      type="button"
                      className="btn btn-sm btn-outline-secondary d-inline-flex align-items-center gap-1"
                      onClick={handleLogout}
                    >
                      <img src={logoutIcon} alt="" width={20} height={20} className="flex-shrink-0" />
                      Logout
                    </button>
                  </>
                ) : (
                  <Link
                    className="btn btn-sm btn-primary d-inline-flex align-items-center gap-1"
                    to="/login"
                  >
                    <img src={loginIcon} alt="" width={20} height={20} className="flex-shrink-0" />
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
          &copy; 2026 - Positive News
          {location.pathname !== '/privacy' ? (
            <>
              {' '}
              -{' '}
              <Link
                to="/privacy"
                className="text-muted"
                state={feedSearchForPrivacy ? { feedSearch: feedSearchForPrivacy } : undefined}
              >
                Privacy
              </Link>
            </>
          ) : null}
        </div>
      </footer>
    </>
  )
}
