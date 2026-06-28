import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AdminNavBar } from './AdminNavBar'

describe('AdminNavBar', () => {
  it('highlights the active section', () => {
    const onSelect = vi.fn()
    render(<AdminNavBar activeSection="sources" onSelect={onSelect} />)

    expect(screen.getByRole('button', { name: 'Sources' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('button', { name: 'Articles' })).toHaveAttribute('aria-pressed', 'false')
  })

  it('calls onSelect when a section is clicked', async () => {
    const user = userEvent.setup()
    const onSelect = vi.fn()
    render(<AdminNavBar activeSection="sources" onSelect={onSelect} />)

    await user.click(screen.getByRole('button', { name: 'Users' }))

    expect(onSelect).toHaveBeenCalledWith('users')
  })
})
