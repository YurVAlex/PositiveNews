/**
 * Renders article preview images parsed from HTML img tags with lazy loading and source fallbacks.
 */
import { useEffect, useState } from 'react'

type ArticleImageProps = {
  imageTag: string | null | undefined
  /** Source-specific fallback from wwwroot/Defaults/ when preview is missing or fails to load. */
  fallbackImageTag?: string | null
  index: number
}

type ParsedImg = {
  src: string
  alt: string
  className: string
}

/** Extracts src, alt, and class from a stored HTML img tag string. */
function parseImgFromTag(imageTag: string | null | undefined): ParsedImg | null {
  if (!imageTag?.trim()) {
    return null
  }

  const doc = new DOMParser().parseFromString(imageTag, 'text/html')
  const img = doc.querySelector('img')
  const src = img?.getAttribute('src')?.trim()
  if (!img || !src) {
    return null
  }

  return {
    src,
    alt: img.getAttribute('alt') ?? '',
    className: img.getAttribute('class') ?? 'img-fluid w-100',
  }
}

export function ArticleImage({ imageTag, fallbackImageTag, index }: ArticleImageProps) {
  const primary = parseImgFromTag(imageTag)
  const fallback = parseImgFromTag(fallbackImageTag)
  const [useFallback, setUseFallback] = useState(false)

  // Reset fallback when the article image changes (e.g. navigating between cards).
  useEffect(() => {
    setUseFallback(false)
  }, [imageTag, fallbackImageTag])

  const active = useFallback || !primary ? fallback : primary
  if (!active) {
    return null
  }

  const loading = index < 2 ? 'eager' : 'lazy'
  const decoding = index < 2 ? 'sync' : 'async'

  return (
    <img
      src={active.src}
      alt={active.alt}
      className={active.className}
      loading={loading}
      decoding={decoding}
      onError={() => {
        // Swap to source default image when the primary preview fails to load.
        if (fallback && active.src !== fallback.src) {
          setUseFallback(true)
        }
      }}
    />
  )
}
