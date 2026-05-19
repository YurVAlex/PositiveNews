import { fireEvent, render, screen } from '@testing-library/react'
import { ArticleImage } from './ArticleImage'

describe('ArticleImage', () => {
  it('renders primary image from imageTag', () => {
    render(
      <ArticleImage
        imageTag='<img src="https://example.com/hero.jpg" alt="Hero" class="hero" />'
        index={0}
      />,
    )

    const img = screen.getByRole('img', { name: 'Hero' })
    expect(img).toHaveAttribute('src', 'https://example.com/hero.jpg')
    expect(img).toHaveAttribute('loading', 'eager')
  })

  it('uses fallback when imageTag is missing', () => {
    render(
      <ArticleImage
        imageTag={null}
        fallbackImageTag='<img src="/Defaults/nasa.png" alt="Default article image" />'
        index={3}
      />,
    )

    const img = screen.getByRole('img', { name: 'Default article image' })
    expect(img).toHaveAttribute('src', '/Defaults/nasa.png')
    expect(img).toHaveAttribute('loading', 'lazy')
  })

  it('switches to fallback when primary image fails to load', () => {
    render(
      <ArticleImage
        imageTag='<img src="https://example.com/broken.jpg" alt="Hero" />'
        fallbackImageTag='<img src="/Defaults/nvidia.png" alt="Default article image" />'
        index={0}
      />,
    )

    const img = screen.getByRole('img', { name: 'Hero' })
    fireEvent.error(img)
    expect(screen.getByRole('img', { name: 'Default article image' })).toHaveAttribute(
      'src',
      '/Defaults/nvidia.png',
    )
  })
})
