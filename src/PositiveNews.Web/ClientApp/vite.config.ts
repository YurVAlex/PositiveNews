import { readFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const __dirname = dirname(fileURLToPath(import.meta.url))

/** Dev API target: VITE_DEV_API_PROXY_TARGET override, else HTTP URL from launchSettings.json. */
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

  const httpUrl = applicationUrl.split(';').find((u) => u.trim().startsWith('http://'))
  if (!httpUrl) {
    throw new Error('launchSettings.json: no HTTP URL found in applicationUrl.')
  }

  return httpUrl.trim()
}

// Production build writes into wwwroot next to this file (preserves existing /lib, /css from the host).
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
      '/api': { target: resolveDevApiProxyTarget(), changeOrigin: true },
    },
  },
})
