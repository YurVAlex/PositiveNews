import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { FeedActiveSources } from './FeedActiveSources'

describe('FeedActiveSources', () => {
  it('renders nothing when sources are empty', () => {
    const { container } = render(
      <MemoryRouter>
        <FeedActiveSources sources={[]} buildSourceToggleUrl={(id) => String(id)} />
      </MemoryRouter>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('renders active source chips with logos and optional hint', () => {
    render(
      <MemoryRouter>
        <FeedActiveSources
          sources={[
            { id: 1, name: 'Alpha', logoUrl: 'https://example.com/a.png' },
            { id: 2, name: 'Beta', logoUrl: null },
          ]}
          buildSourceToggleUrl={(id) => `/feed?source=${id}`}
          hint="Source hint"
        />
      </MemoryRouter>,
    )

    expect(screen.getByText('Source hint')).toBeInTheDocument()
    expect(screen.getByText('Active sources:')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Alpha/ })).toHaveAttribute('href', '/feed?source=1')
    expect(screen.getByRole('link', { name: /Beta/ })).toHaveAttribute('href', '/feed?source=2')
  })
})
