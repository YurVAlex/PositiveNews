import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FeedPagination } from './FeedPagination'

describe('FeedPagination', () => {
  it('renders nothing when there is only one page', () => {
    const { container } = render(
      <FeedPagination currentPage={1} totalPages={1} onPageChange={() => {}} />,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('navigates with previous and next buttons', async () => {
    const user = userEvent.setup()
    const onPageChange = vi.fn()

    render(<FeedPagination currentPage={2} totalPages={5} onPageChange={onPageChange} />)

    await user.click(screen.getByRole('button', { name: 'Previous' }))
    expect(onPageChange).toHaveBeenCalledWith(1)

    await user.click(screen.getByRole('button', { name: 'Next' }))
    expect(onPageChange).toHaveBeenCalledWith(3)
  })

  it('commits page from input on enter and blur', async () => {
    const user = userEvent.setup()
    const onPageChange = vi.fn()

    render(<FeedPagination currentPage={2} totalPages={5} onPageChange={onPageChange} />)

    const input = screen.getByRole('textbox', { name: 'Current page' })
    await user.clear(input)
    await user.type(input, '4{Enter}')
    expect(onPageChange).toHaveBeenCalledWith(4)

    onPageChange.mockClear()
    await user.clear(input)
    await user.type(input, '99')
    await user.tab()
    expect(onPageChange).toHaveBeenCalledWith(5)
    expect(input).toHaveValue('5')
  })

  it('reverts invalid input to the current page on blur', async () => {
    const user = userEvent.setup()
    const onPageChange = vi.fn()

    render(<FeedPagination currentPage={2} totalPages={5} onPageChange={onPageChange} />)

    const input = screen.getByRole('textbox', { name: 'Current page' })
    await user.clear(input)
    await user.type(input, 'abc')
    await user.tab()

    expect(onPageChange).not.toHaveBeenCalled()
    expect(input).toHaveValue('2')
  })
})
