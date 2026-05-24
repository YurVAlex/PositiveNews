import { useCallback, useEffect, useRef, useState } from 'react'
import {
  fetchIngestionRuns,
  fetchIngestionStatus,
  triggerIngestionCycle,
  type IngestionCycleStatus,
  type IngestionRunListItem,
} from '../../api/admin-ingestion-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'

const STATUS_POLL_MS = 15_000

function nextRunLabel(status: IngestionCycleStatus | null): string {
  if (!status) return 'Loading…'
  if (status.isRunning) return 'In progress'
  return formatApiUtcAsLocal(status.nextRunAtUtc)
}

export function IngestionRuns() {
  const { token } = useAuth()
  const [status, setStatus] = useState<IngestionCycleStatus | null>(null)
  const [statusError, setStatusError] = useState<string | null>(null)
  const [runs, setRuns] = useState<IngestionRunListItem[]>([])
  const [runsError, setRunsError] = useState<string | null>(null)
  const [tableVisible, setTableVisible] = useState(false)
  const [runsLoading, setRunsLoading] = useState(false)
  const [actionMessage, setActionMessage] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [triggerLoading, setTriggerLoading] = useState(false)
  const previousStatusRef = useRef<IngestionCycleStatus | null>(null)

  const refreshStatus = useCallback(async () => {
    if (!token) return
    try {
      const next = await fetchIngestionStatus(token)
      setStatus(next)
      setStatusError(null)
    } catch (err) {
      setStatusError(err instanceof Error ? err.message : 'Failed to load ingestion status')
    }
  }, [token])

  useEffect(() => {
    void refreshStatus()
    const id = window.setInterval(() => void refreshStatus(), STATUS_POLL_MS)
    return () => window.clearInterval(id)
  }, [refreshStatus])

  const handleShowRuns = useCallback(async () => {
    if (!token) return
    setRunsLoading(true)
    setRunsError(null)
    try {
      const items = await fetchIngestionRuns(token)
      setRuns(items)
      setTableVisible(true)
    } catch (err) {
      setRunsError(err instanceof Error ? err.message : 'Failed to load ingestion runs')
    } finally {
      setRunsLoading(false)
    }
  }, [token])

  useEffect(() => {
    const previousStatus = previousStatusRef.current
    if (previousStatus?.isRunning && status && !status.isRunning) {
      setActionMessage(null)
      if (tableVisible) {
        void handleShowRuns()
      }
    }
    previousStatusRef.current = status
  }, [status, tableVisible, handleShowRuns])

  const handleTrigger = async () => {
    if (!token || status?.isRunning) return
    setTriggerLoading(true)
    setActionMessage(null)
    setActionError(null)
    try {
      await triggerIngestionCycle(token)
      setActionMessage('Ingestion cycle started.')
      await refreshStatus()
    } catch (err) {
      setActionError(err instanceof Error ? err.message : 'Failed to start ingestion cycle')
    } finally {
      setTriggerLoading(false)
    }
  }

  return (
    <section className="card mb-4">
      <div className="card-body">
        <h2 className="h5 card-title mb-3">Ingestion</h2>

        <p className="mb-3">
          <strong>Next run at:</strong> {statusError ? statusError : nextRunLabel(status)}
        </p>

        {actionMessage ? (
          <div className="alert alert-success py-2" role="status">
            {actionMessage}
          </div>
        ) : null}
        {actionError ? (
          <div className="alert alert-danger py-2" role="alert">
            {actionError}
          </div>
        ) : null}
        {runsError ? (
          <div className="alert alert-danger py-2" role="alert">
            {runsError}
          </div>
        ) : null}

        <div className="d-flex flex-wrap gap-2 mb-3">
          <button
            type="button"
            className="btn btn-primary"
            disabled={!token || triggerLoading || status?.isRunning === true}
            onClick={() => void handleTrigger()}
          >
            {triggerLoading ? 'Starting…' : 'Launch ingestion cycle'}
          </button>
          <button
            type="button"
            className="btn btn-outline-secondary"
            disabled={!token || runsLoading}
            onClick={() => void handleShowRuns()}
          >
            {runsLoading ? 'Loading…' : 'Show Ingestion Runs'}
          </button>
        </div>

        {tableVisible ? (
          <>
            <div
              className="table-responsive border rounded mb-2"
              style={{ maxHeight: '24rem', overflowY: 'auto' }}
            >
              <table className="table table-sm table-striped mb-0">
                <thead className="table-light sticky-top">
                  <tr>
                    <th scope="col">Id</th>
                    <th scope="col">Source</th>
                    <th scope="col">Started at</th>
                    <th scope="col">Finished at</th>
                    <th scope="col">Status</th>
                    <th scope="col">Items fetched</th>
                  </tr>
                </thead>
                <tbody>
                  {runs.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="text-muted">
                        No ingestion runs found.
                      </td>
                    </tr>
                  ) : (
                    runs.map((run) => (
                      <tr key={run.id}>
                        <td>{run.id}</td>
                        <td>{run.sourceName}</td>
                        <td>{formatApiUtcAsLocal(run.startedAt)}</td>
                        <td>{formatApiUtcAsLocal(run.finishedAt)}</td>
                        <td>{run.status}</td>
                        <td>{run.itemsFetched}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
            <button
              type="button"
              className="btn btn-outline-secondary btn-sm"
              onClick={() => setTableVisible(false)}
            >
              Hide table
            </button>
          </>
        ) : null}
      </div>
    </section>
  )
}
