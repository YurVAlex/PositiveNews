/** Admin dashboard: verifies API access, then renders moderation and ops panels. */
import { useEffect, useState } from 'react'
import { fetchAdminStatus } from '../api/admin-api'
import { IngestionRuns } from '../components/admin/IngestionRuns'
import { SourcesModeration } from '../components/admin/SourcesModeration'
import { ArticlesModeration } from '../components/admin/ArticlesModeration'
import { ManageUsers } from '../components/admin/ManageUsers'
import { ManageComments } from '../components/admin/ManageComments'
import { AuditLogs } from '../components/admin/AuditLogs'
import { useAuth } from '../auth/AuthProvider'

export function AdminPage() {
  const { token } = useAuth()
  const [accessOk, setAccessOk] = useState<boolean | null>(null)
  const [error, setError] = useState<string | null>(null)

  // Route may be reachable in the SPA; this call confirms the token has admin rights on the server.
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
      <div className="d-flex justify-content-between align-items-center mb-3">
  <h1 className="h3 mb-0">Admin panel</h1>

  {error ? (
    <div className="alert alert-danger mb-0">{error}</div>
  ) : accessOk ? (
    <p className="text-muted small mb-0">Admin API access confirmed.</p>
  ) : (
    <p className="text-muted small mb-0">Checking admin API access…</p>
  )}
</div>

      {accessOk ? (
        <>
          <SourcesModeration />
          <ArticlesModeration />
          <ManageUsers />
          <ManageComments />
          <AuditLogs />
          <IngestionRuns />
        </>
      ) : null}
    </main>
  )
}
