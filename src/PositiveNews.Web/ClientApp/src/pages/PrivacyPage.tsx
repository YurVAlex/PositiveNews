import { useEffect } from 'react'

export function PrivacyPage() {
  useEffect(() => {
    document.title = 'Privacy Policy - PositiveNews.Web'
  }, [])

  return (
    <main className="pb-3 mt-4">
      <h1>Privacy Policy</h1>
      <p>Use this page to detail your site&apos;s privacy policy.</p>
    </main>
  )
}
