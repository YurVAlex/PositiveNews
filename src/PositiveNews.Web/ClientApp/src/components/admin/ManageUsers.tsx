import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import {
  fetchAdminUserDetail,
  fetchAdminUsers,
  type AdminUserDetail,
  type AdminUserItem,
  type UpdateUserRequest,
  updateAdminUser,
} from '../../api/admin-users-api'
import { useAuth } from '../../auth/AuthProvider'
import { formatApiUtcAsLocal } from '../../utils/format-api-datetime'

export function ManageUsers() {
  const { token } = useAuth()
  const [users, setUsers] = useState<AdminUserItem[]>([])
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null)
  const [selectedUserDetail, setSelectedUserDetail] = useState<AdminUserDetail | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [searchPerformed, setSearchPerformed] = useState(false)
  const [loading, setLoading] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)
  const [submitLoading, setSubmitLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)
  const [formState, setFormState] = useState<UpdateUserRequest>({
    isActive: false,
    emailConfirmed: false,
    reason: '',
    note: '',
  })

  useEffect(() => {
    if (selectedUserId === null || !token) {
      setSelectedUserDetail(null)
      return
    }

    setDetailLoading(true)
    setError(null)
    void fetchAdminUserDetail(token, selectedUserId)
      .then((detail) => {
        setSelectedUserDetail(detail)
        setFormState({
          isActive: detail.isActive,
          emailConfirmed: detail.emailConfirmed,
          reason: '',
          note: '',
        })
        setSubmitMessage(null)
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Failed to load user details'))
      .finally(() => setDetailLoading(false))
  }, [selectedUserId, token])

  const handleSearch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token) return

    setError(null)
    setLoading(true)
    setSubmitMessage(null)

    try {
      const items = await fetchAdminUsers(token, searchTerm.trim())
      setUsers(items)
      setSelectedUserId(null)
      setSelectedUserDetail(null)
      setSearchPerformed(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search request failed')
    } finally {
      setLoading(false)
    }
  }

  const handleSelectUser = (id: number) => {
    setSelectedUserId(id)
    setSubmitMessage(null)
    setError(null)
  }

  const handleClearSelection = () => {
    setError(null)
    setSubmitMessage(null)
    setSearchTerm('')
    setSearchPerformed(false)
    setSelectedUserId(null)
    setSelectedUserDetail(null)
    setUsers([])
    setFormState({
      isActive: false,
      emailConfirmed: false,
      reason: '',
      note: '',
    })
  }

  const handleClose = () => {
    setSelectedUserId(null)
    setSelectedUserDetail(null)
    setSubmitMessage(null)
    setError(null)
    setFormState({
      isActive: false,
      emailConfirmed: false,
      reason: '',
      note: '',
    })
  }

  const handleFormChange = (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement
    const value = target.type === 'checkbox'
      ? ('checked' in target ? target.checked : false)
      : target.value

    setFormState((current) => ({
      ...current,
      [target.name]: value,
    }))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (!token || selectedUserId === null) return

    setSubmitLoading(true)
    setError(null)
    setSubmitMessage(null)

    try {
      await updateAdminUser(token, selectedUserId, formState)
      setSubmitMessage('User updated successfully.')
      void fetchAdminUsers(token, searchTerm.trim()).then((items) => setUsers(items))
      void fetchAdminUserDetail(token, selectedUserId).then((detail) => {
        setSelectedUserDetail(detail)
        setFormState({
          isActive: detail.isActive,
          emailConfirmed: detail.emailConfirmed,
          reason: '',
          note: '',
        })
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save user changes')
    } finally {
      setSubmitLoading(false)
    }
  }

  return (
    <section className="card mb-4">
      <div className="card-body">
        <div className="d-flex align-items-start justify-content-between mb-3">
          <div>
            <h2 className="h5 card-title mb-1">Moderation of users</h2>
            <p className="small text-muted mb-0">Search by id or name and update account state.</p>
          </div>
        </div>

        {error ? (
          <div className="alert alert-danger py-2" role="alert">{error}</div>
        ) : null}

        {submitMessage ? (
          <div className="alert alert-success py-2" role="status">{submitMessage}</div>
        ) : null}

        <form className="mb-3" onSubmit={handleSearch}>
          <div className="input-group">
            <input
              type="search"
              className="form-control"
              placeholder="Search users by id or name"
              value={searchTerm}
              onChange={(event) => setSearchTerm(event.target.value)}
            />
            <button type="submit" className="btn btn-outline-secondary" disabled={!token || loading}>
              {loading ? 'Searching…' : 'Search'}
            </button>
          </div>
        </form>

        <div className="mb-3">
          <button type="button" className="btn btn-sm btn-outline-secondary" onClick={handleClearSelection} disabled={!token || loading}>
            Clear
          </button>
        </div>

        {searchPerformed ? (
          <div className="row g-3">
            <div className="col-12">
              <div className="table-responsive border rounded mb-3" style={{ maxHeight: '26rem', overflowY: 'auto' }}>
                <table className="table table-sm table-hover mb-0">
                  <thead className="table-light">
                    <tr>
                      <th scope="col">Id</th>
                      <th scope="col">Name</th>
                      <th scope="col">Active</th>
                      <th scope="col">Email confirmed</th>
                      <th scope="col">FailedLogin</th>
                      <th scope="col">Registration date</th>
                      <th scope="col">Moderated</th>
                    </tr>
                  </thead>
                  <tbody>
                    {loading ? (
                      <tr><td colSpan={7} className="text-muted">Loading users…</td></tr>
                    ) : users.length === 0 ? (
                      <tr><td colSpan={7} className="text-muted">No users found.</td></tr>
                    ) : (
                      users.map((user) => (
                        <tr key={user.id} className={user.id === selectedUserId ? 'table-primary' : undefined} role="button" onClick={() => handleSelectUser(user.id)}>
                          <td>{user.id}</td>
                          <td>{user.name}</td>
                          <td>{user.isActive ? 'Yes' : 'No'}</td>
                          <td>{user.emailConfirmed ? 'Yes' : 'No'}</td>
                          <td>{user.failedLoginCount}</td>
                          <td>{formatApiUtcAsLocal(user.createdAt)}</td>
                          <td>{user.moderatedBy != null ? 'Yes' : 'No'}</td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="col-12">
              <div className="border rounded p-3 mt-3">
                <div className="d-flex align-items-center justify-content-between mb-3">
                  <h3 className="h6 mb-0">Moderation details</h3>
                </div>

                {selectedUserId === null ? (
                  <p className="text-muted">Select a user to review and update account state.</p>
                ) : detailLoading ? (
                  <p className="text-muted">Loading user details…</p>
                ) : selectedUserDetail === null ? (
                  <p className="text-muted">No user details available.</p>
                ) : (
                  <form onSubmit={handleSubmit}>
                    <div className="mb-3">
                      <label className="form-label">Name</label>
                      <input type="text" className="form-control" value={selectedUserDetail.name} disabled />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Email</label>
                      <input type="text" className="form-control" value={selectedUserDetail.email} disabled />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Last login date</label>
                      <input type="text" className="form-control" value={selectedUserDetail.lastLoginAt ? formatApiUtcAsLocal(selectedUserDetail.lastLoginAt) : ''} disabled />
                    </div>
                    <div className="mb-3 form-check">
                      <input id="isActive" name="isActive" type="checkbox" className="form-check-input" checked={formState.isActive} onChange={handleFormChange} />
                      <label className="form-check-label" htmlFor="isActive">Is active</label>
                    </div>
                    <div className="mb-3 form-check">
                      <input id="emailConfirmed" name="emailConfirmed" type="checkbox" className="form-check-input" checked={formState.emailConfirmed} onChange={handleFormChange} />
                      <label className="form-check-label" htmlFor="emailConfirmed">Email confirmed</label>
                    </div>
                    <div className="mb-3">
                      <label className="form-label" htmlFor="reason">Reason</label>
                      <textarea id="reason" name="reason" className="form-control" value={formState.reason ?? ''} onChange={handleFormChange} rows={2} placeholder="Optional moderation reason" />
                    </div>
                    <div className="mb-3">
                      <label className="form-label" htmlFor="note">Notes</label>
                      <textarea id="note" name="note" className="form-control" value={formState.note ?? ''} onChange={handleFormChange} rows={3} placeholder="Optional note for audit log" />
                    </div>
                    <div className="d-flex gap-2">
                      <button type="submit" className="btn btn-primary" disabled={submitLoading}>{submitLoading ? 'Applying…' : 'Apply'}</button>
                      <button type="button" className="btn btn-outline-secondary" onClick={handleClose} disabled={submitLoading}>Close</button>
                    </div>
                  </form>
                )}
              </div>
            </div>
          </div>
        ) : (
          <p className="text-muted mb-0">Search for users to manage account state.</p>
        )}
      </div>
    </section>
  )
}