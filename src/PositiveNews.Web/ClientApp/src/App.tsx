import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from './auth/ProtectedRoute'
import { AppLayout } from './components/AppLayout'
import { FeedPage } from './pages/FeedPage'
import { ArticleDetailPage } from './pages/ArticleDetailPage'
import { PrivacyPage } from './pages/PrivacyPage'
import { LoginPage } from './pages/LoginPage'
import { AdminPage } from './pages/AdminPage'
import { RegisterPage } from './pages/RegisterPage'

export function App() {
  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<FeedPage />} />
        <Route path="/articles/:id" element={<ArticleDetailPage />} />
        <Route path="/privacy" element={<PrivacyPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/admin" element={<ProtectedRoute element={<AdminPage />} roles={['Admin']} />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AppLayout>
  )
}
