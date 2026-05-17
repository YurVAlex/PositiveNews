import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { FeedActiveTopics } from './FeedActiveTopics'

describe('FeedActiveTopics', () => {
  it('renders nothing when topics are empty', () => {
    const { container } = render(
      <MemoryRouter>
        <FeedActiveTopics topics={[]} buildTopicToggleUrl={(t) => t} />
      </MemoryRouter>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('renders active topic chips and optional hint', () => {
    render(
      <MemoryRouter>
        <FeedActiveTopics
          topics={['Health', 'Science']}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          hint="Sort hint"
        />
      </MemoryRouter>,
    )

    expect(screen.getByText('Sort hint')).toBeInTheDocument()
    expect(screen.getByText('Active topics:')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Health' })).toHaveAttribute('href', '/feed?topic=Health')
    expect(screen.getByRole('link', { name: 'Science' })).toHaveAttribute('href', '/feed?topic=Science')
  })
})
