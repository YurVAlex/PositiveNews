import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { FeedPreferenceMobileSelect } from './FeedPreferenceMobileSelect'

describe('FeedPreferenceMobileSelect', () => {
  it('adds a topic without clearing existing selections', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()

    render(
      <FeedPreferenceMobileSelect
        ariaLabel="Select topics"
        options={[
          { value: 'Health', label: 'Health' },
          { value: 'Science', label: 'Science' },
        ]}
        selectedValues={['Health']}
        onChange={onChange}
        equals={(a, b) => a.toLowerCase() === b.toLowerCase()}
      />,
    )

    await user.click(screen.getByRole('option', { name: 'Science' }))

    expect(onChange).toHaveBeenCalledWith(['Health', 'Science'])
  })

  it('removes a topic when clicking a selected option with × label', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()

    render(
      <FeedPreferenceMobileSelect
        ariaLabel="Select topics"
        options={[
          { value: 'Health', label: 'Health' },
          { value: 'Science', label: 'Science' },
        ]}
        selectedValues={['Health', 'Science']}
        onChange={onChange}
        equals={(a, b) => a.toLowerCase() === b.toLowerCase()}
      />,
    )

    await user.click(screen.getByRole('option', { name: 'Health ×' }))

    expect(onChange).toHaveBeenCalledWith(['Science'])
  })

  it('marks active options with bold styling class', () => {
    render(
      <FeedPreferenceMobileSelect
        ariaLabel="Select topics"
        options={[{ value: 'Health', label: 'Health' }]}
        selectedValues={['Health']}
        onChange={vi.fn()}
      />,
    )

    const healthOption = screen.getByRole('option', { name: 'Health ×' })
    expect(healthOption).toHaveClass('feed-preference-select__option--active')
    expect(healthOption).toHaveAttribute('aria-selected', 'true')
  })
})
