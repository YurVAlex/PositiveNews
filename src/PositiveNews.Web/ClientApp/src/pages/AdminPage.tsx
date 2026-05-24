import { useEffect, useState } from 'react'
import { fetchAdminStatus } from '../api/admin-api'
import { IngestionRuns } from '../components/admin/IngestionRuns'
import { ManageSources } from '../components/admin/ManageSources'
import { useAuth } from '../auth/AuthProvider'

export function AdminPage() {
  const { token } = useAuth()
  const [accessOk, setAccessOk] = useState<boolean | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!token) {
      setError('Unauthorized')
      setAccessOk(false)
      return
    }

    fetchAdminStatus(token)
      .then(() => {
        setAccessOk(true)
        setError(null)
      })
      .catch((err) => {
        setAccessOk(false)
        setError(err instanceof Error ? err.message : 'Failed to reach admin endpoint')
      })
  }, [token])

  return (
    <main role="main" className="pb-3 mt-4">
      <h1 className="h3 mb-3">Admin panel</h1>

      {error ? (
        <div className="alert alert-danger mb-3">{error}</div>
      ) : accessOk ? (
        <p className="text-muted small mb-3">Admin API access confirmed.</p>
      ) : (
        <p className="text-muted small mb-3">Checking admin API access…</p>
      )}

      {accessOk ? (
        <>
          <ManageSources />
          <IngestionRuns />
        </>
      ) : null}
    </main>
  )
}
