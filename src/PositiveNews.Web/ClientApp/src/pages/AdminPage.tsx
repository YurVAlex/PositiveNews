/** Admin dashboard: verifies API access, then renders one moderation section at a time. */
import { useEffect, useState } from 'react'
import { fetchAdminStatus } from '../api/admin-api'
import { AdminNavBar } from '../components/admin/AdminNavBar'
import { ADMIN_SECTIONS, type AdminSectionId } from '../components/admin/admin-sections'
import { useAuth } from '../auth/AuthProvider'

export function AdminPage() {
  const { token } = useAuth()
  const [accessOk, setAccessOk] = useState<boolean | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [activeSection, setActiveSection] = useState<AdminSectionId>('sources')

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

  const activeConfig = ADMIN_SECTIONS.find((section) => section.id === activeSection)
  const ActiveComponent = activeConfig?.Component

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
          <AdminNavBar activeSection={activeSection} onSelect={setActiveSection} />
          {ActiveComponent ? (
            <section className="card mb-4">
              <div className="card-body">
                <ActiveComponent />
              </div>
            </section>
          ) : null}
        </>
      ) : null}
    </main>
  )
}
