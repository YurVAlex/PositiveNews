/** Sign-in form; redirects authenticated users and honors a post-login return path. */
import { FormEvent, useState } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'

type LoginLocationState = {
  from?: string
}

export function LoginPage() {
  const { isAuthenticated, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  // Protected routes set location.state.from so login sends the user back where they came from.
  const state = (location.state as LoginLocationState | null) ?? null
  const redirectTo = state?.from ?? '/'

  const [email, setEmail] = useState('admin@positivenews.local')
  const [password, setPassword] = useState('Admin123!')
  const [showPassword, setShowPassword] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (isAuthenticated && !isSubmitting) {
    // Already signed in—skip the form and go to the intended destination.
    // Do not redirect while login() is still loading prefs (token/user are set first).
    return <Navigate to={redirectTo} replace />
  }

  const onSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    // AuthProvider stores the token; navigate completes the redirect after a successful login.
    setError(null)
    setIsSubmitting(true)
    try {
      await login(email, password)
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main role="main" className="pb-3 mt-4">
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-5">
          <h3 className="mb-3">Sign in</h3>
          <form onSubmit={onSubmit} className="card card-body shadow-sm">
            <div className="mb-3">
              <label htmlFor="email" className="form-label">
                Email
              </label>
              <input
                id="email"
                type="email"
                className="form-control"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>

            <div className="mb-3">
              <label htmlFor="password" className="form-label">
                Password
              </label>
              <div className="input-group">
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  className="form-control"
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={() => setShowPassword((visible) => !visible)}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                  aria-pressed={showPassword}
                >
                  {showPassword ? 'Hide' : 'Show'}
                </button>
              </div>
            </div>

            {error ? <div className="alert alert-danger py-2">{error}</div> : null}

            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              {isSubmitting ? 'Signing in...' : 'Sign in'}
            </button>
          </form>
                  <div className="mt-3 text-muted fs-5 fw-medium">
            No account yet? <Link to="/register">Register</Link>
          </div>
        </div>
      </div>
    </main>
  )
}
