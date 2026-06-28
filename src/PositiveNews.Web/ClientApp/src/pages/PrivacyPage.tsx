/** Static privacy copy; signed-in users can permanently deactivate their account here. */
import { useEffect, useState, type MouseEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { deactivateAccount } from '../api/auth-api'
import { useAuth } from '../auth/AuthProvider'

export function PrivacyPage() {
  const { isAuthenticated, token, logout } = useAuth()
  const navigate = useNavigate()
  const [isDeactivating, setIsDeactivating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    document.title = 'Privacy Policy - PositiveNews.Web'
  }, [])

  // Soft-delete on the server, then clear auth and session feed preferences and return to the default feed.
  const onDeactivateClick = async (e: MouseEvent<HTMLAnchorElement>) => {
    e.preventDefault()
    if (!token || isDeactivating) {
      return
    }

    const confirmed = window.confirm(
      'Are you sure you want to delete your account? You will not be able to sign in again.',
    )
    if (!confirmed) {
      return
    }

    setError(null)
    setIsDeactivating(true)
    try {
      await deactivateAccount(token)
      logout()
      navigate({ pathname: '/', search: '' }, { replace: true, state: null })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Account deactivation failed.')
    } finally {
      setIsDeactivating(false)
    }
  }

  return (
    <main className="pb-3 mt-4">
      <p>All rights to publications belong to the issuing resources and their authors.</p>
      <p>
        Developed by <a href="https://github.com/YurVAlex">YurVAlex</a> as academic project for{' '}
        <a href="https://www.it-academy.by">IT-Academy</a>.
          </p>
          <h1>User section</h1>
          <p>We respect your privacy. We do not distribute your data.</p>
      {isAuthenticated && (
        <p className="mt-4">
          Want to delete your account?{' '}
          <a href="#" onClick={onDeactivateClick} aria-disabled={isDeactivating}>
            {isDeactivating ? 'Deactivating…' : 'Click that link'}
          </a>
          .
        </p>
      )}
      {error && (
        <p className="text-danger" role="alert">
          {error}
        </p>
      )}
    </main>
  )
}
