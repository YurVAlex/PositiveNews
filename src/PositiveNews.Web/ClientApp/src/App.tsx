import { Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { FeedPage } from './pages/FeedPage'
import { ArticleDetailPage } from './pages/ArticleDetailPage'
import { PrivacyPage } from './pages/PrivacyPage'

export function App() {
  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<FeedPage />} />
        <Route path="/articles/:id" element={<ArticleDetailPage />} />
        <Route path="/privacy" element={<PrivacyPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AppLayout>
  )
}
