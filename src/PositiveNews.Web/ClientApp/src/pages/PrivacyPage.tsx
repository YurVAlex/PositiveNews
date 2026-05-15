import { useEffect } from 'react'

export function PrivacyPage() {
  useEffect(() => {
    document.title = 'Privacy Policy - PositiveNews.Web'
  }, [])

  return (
    <main className="pb-3 mt-4">
          <h1>Positive News</h1>
          <p>All rights belong to the authors.</p>
    </main>
  )
}
