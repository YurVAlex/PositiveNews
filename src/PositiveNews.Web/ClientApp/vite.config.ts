import { readFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const __dirname = dirname(fileURLToPath(import.meta.url))

/** Dev API target: VITE_DEV_API_PROXY_TARGET override, else HTTPS URL from launchSettings.json (falls back to HTTP). */
function resolveDevApiProxyTarget(): string {
  if (process.env.VITE_DEV_API_PROXY_TARGET) {
    return process.env.VITE_DEV_API_PROXY_TARGET
  }

  const launchSettingsPath = join(__dirname, '../Properties/launchSettings.json')
  const launchSettings = JSON.parse(readFileSync(launchSettingsPath, 'utf-8')) as {
    profiles: Record<string, { applicationUrl?: string }>
  }
  const applicationUrl = launchSettings.profiles['PositiveNews.Web']?.applicationUrl
  if (!applicationUrl) {
    throw new Error("launchSettings.json: profile 'PositiveNews.Web' has no applicationUrl.")
  }

  const urls = applicationUrl.split(';').map((u) => u.trim())
  const httpsUrl = urls.find((u) => u.startsWith('https://'))
  const httpUrl = urls.find((u) => u.startsWith('http://'))
  const target = httpsUrl ?? httpUrl
  if (!target) {
    throw new Error('launchSettings.json: no HTTP or HTTPS URL found in applicationUrl.')
  }

  return target
}

function createAspNetDevProxy() {
  return {
    target: resolveDevApiProxyTarget(),
    changeOrigin: true,
    // Dev HTTPS uses a self-signed cert; proxy stays server-side so the browser never needs CORS.
    secure: false,
  } as const
}

// Production build writes into wwwroot next to this file (preserves Logos, Defaults, and other static host files).
export default defineConfig({
  plugins: [react()],
  appType: 'spa',
  test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    globals: true,
  },
  build: {
    outDir: '../wwwroot',
    emptyOutDir: false,
    assetsDir: 'assets',
  },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': createAspNetDevProxy(),
      // wwwroot static assets (logos, default images) are hosted by ASP.NET, not Vite.
      '/Logos': createAspNetDevProxy(),
      '/Defaults': createAspNetDevProxy(),
      '/favicon.ico': createAspNetDevProxy(),
    },
  },
})
