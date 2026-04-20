type ArticleImageProps = {
  imageTag: string | null | undefined
  index: number
}

/**
 * Mirrors the legacy <article-img /> tag helper: extracts the first <img> from stored HTML
 * and applies loading/decoding hints without altering other attributes.
 */
export function ArticleImage({ imageTag, index }: ArticleImageProps) {
  if (!imageTag?.trim()) {
    return null
  }

  const doc = new DOMParser().parseFromString(imageTag, 'text/html')
  const img = doc.querySelector('img')
  if (!img) {
    return null
  }

  const loading = index < 2 ? 'eager' : 'lazy'
  const decoding = index < 2 ? 'sync' : 'async'
  img.setAttribute('loading', loading)
  img.setAttribute('decoding', decoding)

  return <span className="d-inline-block" dangerouslySetInnerHTML={{ __html: img.outerHTML }} />
}
