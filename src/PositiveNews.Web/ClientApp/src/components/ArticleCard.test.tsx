import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { ArticleCard } from './ArticleCard'
import type { ArticlePreviewResponse } from '../api/types'

const defaultSourceProps = {
  selectedSourceIds: [] as number[],
  buildSourceToggleUrl: (sourceId: number) => `/?source=${sourceId}`,
}

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
          {...defaultSourceProps}
        />
      </MemoryRouter>,
    )

    expect(screen.getByTitle('Source trust score')).toHaveTextContent('1')
    expect(screen.getByTitle('Positivity score')).toHaveClass('text-success')
    expect(screen.getByRole('link', { name: /Health/ })).toHaveClass('btn-primary')
    expect(screen.getByRole('link', { name: 'Science' })).toHaveClass('btn-outline-dark')

    const summary = screen.getByText('A short happy summary.').closest('div')
    expect(summary).toHaveClass('d-none')
    await user.click(screen.getByRole('button', { name: 'Show summary' }))
    expect(summary).not.toHaveClass('d-none')
    expect(screen.getByRole('button', { name: 'Hide summary' })).toBeInTheDocument()
  })

  it('links title to the article detail page', () => {
    render(
      <MemoryRouter>
        <ArticleCard
          article={articlePreview()}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          {...defaultSourceProps}
        />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: 'Good news story' })).toHaveAttribute('href', '/articles/1')
  })

  it('links preview image to the article detail page', () => {
    const article = articlePreview({
      imageTag: '<img src="https://example.com/hero.jpg" alt="Hero" />',
    })

    render(
      <MemoryRouter>
        <ArticleCard
          article={article}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          {...defaultSourceProps}
        />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: 'Read article: Good news story' })).toHaveAttribute(
      'href',
      '/articles/1',
    )
    expect(screen.getByRole('link', { name: 'Read article' })).toHaveAttribute('href', '/articles/1')
  })

  it('links source name to toggle url and highlights when selected', () => {
    const article = articlePreview({ sourceId: 5 })

    render(
      <MemoryRouter>
        <ArticleCard
          article={article}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          selectedSourceIds={[5]}
          buildSourceToggleUrl={(id) => `/?source=${id}`}
        />
      </MemoryRouter>,
    )

    const sourceLink = screen.getByRole('link', { name: /Positive Source/ })
    expect(sourceLink).toHaveAttribute('href', '/?source=5')
    expect(sourceLink).toHaveClass('btn-outline-primary')
    expect(sourceLink.textContent).toContain('×')
  })

  it('shows source default image when preview image is missing', () => {
    render(
      <MemoryRouter>
        <ArticleCard
          article={articlePreview({ imageTag: null, sourceName: 'NASA Breaking News' })}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          {...defaultSourceProps}
        />
      </MemoryRouter>,
    )

    expect(screen.getByRole('link', { name: 'Read article: Good news story' })).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'Default article image' })).toHaveAttribute(
      'src',
      '/Defaults/nasa.png',
    )
  })

  it('renders view count badge with tooltip under positivity score', () => {
    render(
      <MemoryRouter>
        <ArticleCard
          article={articlePreview({ viewCount: 1234, positivityScore: 0.62 })}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          {...defaultSourceProps}
        />
      </MemoryRouter>,
    )

    expect(screen.getByTitle('Views count').textContent?.replace(/\s/g, '')).toContain('1234')
    expect(screen.getByTitle('Positivity score')).toBeInTheDocument()
  })

  it('does not render positivity badge for null score and falls back to unknown author', () => {
    render(
      <MemoryRouter>
        <ArticleCard
          article={articlePreview({ author: ' ', positivityScore: null })}
          index={0}
          selectedTopics={[]}
          buildTopicToggleUrl={(topic) => `/feed?topic=${topic}`}
          {...defaultSourceProps}
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
    sourceId: 1,
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
    viewCount: 0,
    topics: ['Health', 'Science'],
    ...overrides,
  }
}
