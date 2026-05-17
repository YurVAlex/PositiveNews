import { useEffect, useState } from 'react'
import { fetchAdminStatus } from '../api/admin-api'
import { useAuth } from '../auth/AuthProvider'

export function AdminPage() {
  const { token } = useAuth()
  const [message, setMessage] = useState('Checking admin API access...')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) {
      setError('Unauthorized')
      return
    }

    fetchAdminStatus(token)
      .then((response) => {
        setMessage(response.message)
        setError(null)
      })
      .catch((err) => {
        setError(err instanceof Error ? err.message : 'Failed to reach admin endpoint')
      })
  }, [token])

  return (
    <main role="main" className="pb-3 mt-4">
      {error ? (
        <div className="alert alert-danger mb-0">{error}</div>
      ) : (
        <div className="alert alert-success mb-0">{message}</div>
      )}
    </main>
  )
}
