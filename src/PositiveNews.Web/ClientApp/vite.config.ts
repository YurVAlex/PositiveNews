import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

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
      '/api': { target: 'http://localhost:5239', changeOrigin: true },
    },
  },
})
