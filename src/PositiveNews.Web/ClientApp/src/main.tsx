/** Application entry point: mounts React with auth, routing, and global styles. */
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'
import './site-app.css'
import { App } from './App'
import { AuthProvider } from './auth/AuthProvider'

const el = document.getElementById('root')
if (!el) {
  throw new Error('Root element #root not found')
}

// AuthProvider wraps BrowserRouter so route components can call useAuth().
createRoot(el).render(
  <StrictMode>
    <AuthProvider>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </AuthProvider>
  </StrictMode>,
)
