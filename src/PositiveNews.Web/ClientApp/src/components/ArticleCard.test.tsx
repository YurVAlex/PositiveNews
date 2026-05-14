import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { ArticleCard } from './ArticleCard'
import type { ArticlePreviewResponse } from '../api/types'

describe('ArticleCard', () => {
  it('formats positivity, renders topic links, and toggles summary', async () => {
    const user = userEvent.setup()
    const article = articlePreview({ positivityScore: 0.62 })

    render(
      <MemoryRouter>
        <ArticleCard
          article={article}
          index={0}
          selectedTopics={['health']}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
        />
      </MemoryRouter>,
    )

    expect(screen.getByTitle('Source trust score')).toHaveTextContent('1')
    expect(screen.getByText('62% Positivity')).toHaveClass('text-success')
    expect(screen.getByRole('link', { name: 'Health' })).toHaveClass('btn-primary')
    expect(screen.getByRole('link', { name: 'Science' })).toHaveClass('btn-outline-dark')

    const summary = screen.getByText('A short happy summary.').closest('div')
    expect(summary).toHaveClass('d-none')
    await user.click(screen.getByRole('button', { name: 'Show summary' }))
    expect(summary).not.toHaveClass('d-none')
    expect(screen.getByRole('button', { name: 'Hide summary' })).toBeInTheDocument()
  })

  it('does not render positivity badge for null score and falls back to unknown author', () => {
    render(
      <MemoryRouter>
        <ArticleCard
          article={articlePreview({ author: ' ', positivityScore: null })}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
        />
      </MemoryRouter>,
    )

    expect(screen.queryByTitle('Positivity score')).not.toBeInTheDocument()
    expect(screen.getByText(/Unknown Author/)).toBeInTheDocument()
  })
})

function articlePreview(overrides: Partial<ArticlePreviewResponse> = {}): ArticlePreviewResponse {
  return {
    id: 1,
    sourceName: 'Positive Source',
    sourceLogoUrl: null,
    sourceTrustScore: 1,
    title: 'Good news story',
    author: 'Reporter',
    publishedAt: '2026-05-11T00:00:00Z',
    imageTag: null,
    summaryShort: 'A short happy summary.',
    url: 'https://example.com/story',
    positivityScore: 0.5,
    topics: ['Health', 'Science'],
    ...overrides,
  }
}
