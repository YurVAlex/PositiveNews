/**
 * Fallback hero images for articles when the source has no thumbnail in the feed.
 */

/**
 * Default hero images served from wwwroot/Defaults/ (see SeedData.DefaultThumbnailHtml).
 * Keys are normalized source display names from the database.
 */
const SOURCE_DEFAULT_IMAGE_SRC: Record<string, string> = {
  'nvidia blog': '/Defaults/nvidia.png',
  'the optimist daily': '/Defaults/optimistdaily.png',
  'nasa breaking news': '/Defaults/nasa.png',
  'this is colossal news': '/Defaults/thisiscolossal.png',
  'design you trust': '/Defaults/designyoutrust.png',
  'tiny buddha': '/Defaults/buddha.png',
}

/**
 * Returns an img tag for the source default thumbnail, or null when the source has no known default.
 */
export function resolveSourceDefaultImageTag(sourceName: string): string | null {
  const src = SOURCE_DEFAULT_IMAGE_SRC[sourceName.trim().toLowerCase()]
  if (!src) {
    return null
  }

  return `<img src="${src}" alt="Default article image" class="img-fluid w-100 rounded mb-3">`
}
