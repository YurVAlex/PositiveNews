import { useEffect } from 'react'

export function PrivacyPage() {
  useEffect(() => {
    document.title = 'Privacy Policy - PositiveNews.Web'
  }, [])

  return (
    <main className="pb-3 mt-4">
          <h1>Positive News</h1>
          <p>All rights to publications belong to the issuing resources and their authors.</p>
          <p>Developed by <a href="https://github.com/YurVAlex">YurVAlex</a> as academic project for <a href="https://www.it-academy.by">IT-Academy</a>.</p>       
    </main>
  )
}
