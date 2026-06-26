import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { ArticleTopicLinks } from './ArticleTopicLinks'

describe('ArticleTopicLinks', () => {
  it('deduplicates topics, filters blanks, and marks selected topics case-insensitively', () => {
    render(
      <MemoryRouter>
        <ArticleTopicLinks
          topics={['Health', 'Health', ' ', 'Science']}
          selectedTopics={['health']}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
        />
      </MemoryRouter>,
    )

    expect(screen.getAllByRole('link')).toHaveLength(2)
    expect(screen.getByRole('link', { name: /Health/ })).toHaveClass('btn-primary')
    expect(screen.getByRole('link', { name: /Health/ }).textContent).toContain('×')
    expect(screen.getByRole('link', { name: 'Science' })).toHaveAttribute('href', '/feed?topic=Science')
  })

  it('renders nothing when no topics remain', () => {
    const { container } = render(
      <MemoryRouter>
        <ArticleTopicLinks topics={[' ']} selectedTopics={[]} buildTopicToggleUrl={(topic) => topic} />
      </MemoryRouter>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
