import { type FormEvent, useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import {
  getConfirmPasswordError,
  getPasswordValidationErrors,
  PASSWORD_MAX_LENGTH,
  PASSWORD_MIN_LENGTH,
} from '../utils/password-validation'

export function RegisterPage() {
  const { isAuthenticated, register } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [name, setName] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [passwordError, setPasswordError] = useState<string | null>(null)
  const [confirmPasswordError, setConfirmPasswordError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }

  const onSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    setError(null)

    const passwordErrors = getPasswordValidationErrors(password)
    const confirmError = getConfirmPasswordError(password, confirmPassword)

    setPasswordError(passwordErrors[0] ?? null)
    setConfirmPasswordError(confirmError)

    if (passwordErrors.length > 0 || confirmError) {
      return
    }

    setIsSubmitting(true)
    try {
      await register(email, name, password)
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main role="main" className="pb-3 mt-4">
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-5">
          <h3 className="mb-3">Create account</h3>
          <form onSubmit={onSubmit} className="card card-body shadow-sm" noValidate>
            <div className="mb-3">
              <label htmlFor="register-name" className="form-label">
                Name
              </label>
              <input
                id="register-name"
                type="text"
                className="form-control"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
            </div>

            <div className="mb-3">
              <label htmlFor="register-email" className="form-label">
                Email
              </label>
              <input
                id="register-email"
                type="email"
                className="form-control"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>

            <div className="mb-3">
              <label htmlFor="register-password" className="form-label">
                Password
              </label>
              <input
                id="register-password"
                type="password"
                className={`form-control${passwordError ? ' is-invalid' : ''}`}
                required
                minLength={PASSWORD_MIN_LENGTH}
                maxLength={PASSWORD_MAX_LENGTH}
                autoComplete="new-password"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value)
                  if (passwordError) {
                    setPasswordError(getPasswordValidationErrors(e.target.value)[0] ?? null)
                  }
                  if (confirmPasswordError) {
                    setConfirmPasswordError(getConfirmPasswordError(e.target.value, confirmPassword))
                  }
                }}
                aria-invalid={passwordError ? true : undefined}
                aria-describedby={passwordError ? 'register-password-error' : 'register-password-hint'}
              />
              <div id="register-password-hint" className="form-text">
                8–128 characters, with uppercase, lowercase, digit, and special character.
              </div>
              {passwordError ? (
                <div id="register-password-error" className="invalid-feedback d-block">
                  {passwordError}
                </div>
              ) : null}
            </div>

            <div className="mb-3">
              <label htmlFor="register-confirm-password" className="form-label">
                Confirm password
              </label>
              <input
                id="register-confirm-password"
                type="password"
                className={`form-control${confirmPasswordError ? ' is-invalid' : ''}`}
                required
                minLength={PASSWORD_MIN_LENGTH}
                maxLength={PASSWORD_MAX_LENGTH}
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(e) => {
                  setConfirmPassword(e.target.value)
                  if (confirmPasswordError) {
                    setConfirmPasswordError(getConfirmPasswordError(password, e.target.value))
                  }
                }}
                aria-invalid={confirmPasswordError ? true : undefined}
                aria-describedby={confirmPasswordError ? 'register-confirm-password-error' : undefined}
              />
              {confirmPasswordError ? (
                <div id="register-confirm-password-error" className="invalid-feedback d-block">
                  {confirmPasswordError}
                </div>
              ) : null}
            </div>

            {error ? <div className="alert alert-danger py-2">{error}</div> : null}

            <button type="submit" className="btn btn-primary" disabled={isSubmitting}>
              {isSubmitting ? 'Creating account...' : 'Register'}
            </button>
          </form>
          <div className="mt-3 text-muted fs-5 fw-medium">
            Already have an account? <Link to="/login">Sign in</Link>
          </div>
        </div>
      </div>
    </main>
  )
}
