import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { buildPreferenceSortHint, FeedSortSelect, feedSortModeLabel } from './FeedSortSelect'

describe('FeedSortSelect', () => {
  it('renders sort options and calls onSortChange', async () => {
    const user = userEvent.setup()
    const onSortChange = vi.fn()

    render(
      <FeedSortSelect sortMode="date" hasPreferences={true} onSortChange={onSortChange} />,
    )

    const select = screen.getByRole('combobox', { name: 'Sort articles' })
    expect(select).toHaveValue('date')
    expect(screen.getByRole('option', { name: 'Your preferences' })).not.toBeDisabled()

    await user.selectOptions(select, 'positivity')
    expect(onSortChange).toHaveBeenCalledWith('positivity')
  })

  it('disables preferences option when there are no preferences', () => {
    render(
      <FeedSortSelect sortMode="date" hasPreferences={false} onSortChange={() => {}} />,
    )

    expect(screen.getByRole('option', { name: 'Your preferences' })).toBeDisabled()
  })
})

describe('feedSortModeLabel', () => {
  it('maps sort modes to labels', () => {
    expect(feedSortModeLabel('date')).toBe('publication date')
    expect(feedSortModeLabel('positivity')).toBe('positivity score')
    expect(feedSortModeLabel('preferences')).toBe('your preferences')
  })
})

describe('buildPreferenceSortHint', () => {
  it('describes weight sort for preferences mode', () => {
    expect(buildPreferenceSortHint('preferences', 'your preferences')).toContain('1 point')
  })
})
