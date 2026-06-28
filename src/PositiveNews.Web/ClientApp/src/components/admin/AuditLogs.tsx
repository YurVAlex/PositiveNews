/**
 * Admin panel: browse recent moderation audit log entries and view change details.
 */
import { useCallback, useEffect, useState } from 'react'

import { fetchAuditLogs, type AuditLogAdminItem } from '../../api/admin-audit-logs-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'

export function AuditLogs() {
  const { token } = useAuth()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [items, setItems] = useState<AuditLogAdminItem[]>([])
  const [selected, setSelected] = useState<AuditLogAdminItem | null>(null)

  const load = useCallback(async () => {
    if (!token) return
    setLoading(true)
    setError(null)
    try {
      const rows = await fetchAuditLogs(token, 100)
      setItems(rows)
      setSelected(null)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load audit logs')
    } finally {
      setLoading(false)
    }
  }, [token])

  useEffect(() => {
    void load()
  }, [load])

  return (
    <>
      <h2 className="h5 card-title mb-3">Audit logs</h2>

      {error ? (
        <div className="alert alert-danger py-2" role="alert">{error}</div>
      ) : null}

      {loading ? (
        <p className="text-muted">Loading audit logs…</p>
      ) : null}

      <div className="table-responsive border rounded mb-3" style={{ maxHeight: '24rem', overflowY: 'auto' }}>
        <table className="table table-sm table-striped mb-0">
          <thead className="table-light sticky-top">
            <tr>
              <th>Id</th>
              <th>Entity type</th>
              <th>Entity Id</th>
              <th>Changed field</th>
              <th>Moderator Id</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 ? (
              <tr><td colSpan={5} className="text-muted">No audit logs found.</td></tr>
            ) : (
              items.map((it) => (
                <tr
                  key={it.id}
                  role="button"
                  onClick={() => setSelected(it)}
                  className={selected?.id === it.id ? 'table-primary' : undefined}
                >
                  <td>{it.id}</td>
                  <td>{it.entityType}</td>
                  <td>{it.entityId}</td>
                  <td>{it.changedField ?? '-'}</td>
                  <td>{it.moderatorId}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="border rounded p-3">
        <h3 className="h6 mb-2">Details</h3>
        {selected ? (
          <div>
            <p><strong>Changed field:</strong> {selected.changedField ?? '-'}</p>
            <p><strong>Old value:</strong> {selected.oldValue ?? '-'}</p>
            <p><strong>New value:</strong> {selected.newValue ?? '-'}</p>
            <p><strong>Reason:</strong> {selected.reason ?? '-'}</p>
            <p><strong>Note:</strong> {selected.note ?? '-'}</p>
            <p><strong>Created:</strong> {formatApiUtcAsLocal(selected.createdAt)}</p>
          </div>
        ) : (
          <p className="text-muted mb-0">Select a row to view full audit log details.</p>
        )}
      </div>
    </>
  )
}
