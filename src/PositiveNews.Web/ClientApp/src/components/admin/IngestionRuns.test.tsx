import { render, screen, waitFor } from '@testing-library/react'
import { IngestionRuns } from './IngestionRuns'

vi.mock('../../auth/AuthProvider', () => ({
  useAuth: () => ({ token: 'test-token', user: null, isAuthenticated: true }),
}))

vi.mock('../../api/admin-ingestion-api', () => ({
  fetchIngestionStatus: vi.fn().mockResolvedValue({ isRunning: true, nextRunAtUtc: null }),
  fetchIngestionRuns: vi.fn().mockResolvedValue([
    {
      id: 1,
      sourceName: 'Source A',
      startedAt: '2026-05-22T10:00:00Z',
      finishedAt: '2026-05-22T10:01:00Z',
      status: 'Success',
      itemsFetched: 3,
    },
  ]),
  triggerIngestionCycle: vi.fn(),
}))

describe('IngestionRuns', () => {
  it('hides table by default', () => {
    render(<IngestionRuns />)

    expect(screen.queryByRole('table')).not.toBeInTheDocument()
  })

  it('disables launch button when cycle is running', async () => {
    render(<IngestionRuns />)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Launch ingestion cycle' })).toBeDisabled()
    })
  })
})
