import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'node:path'
import autoprefixer from 'autoprefixer'

const isE2E = process.env.VERTICALSLICE_E2E === 'true'
const port = 5173

export default defineConfig({
  base: './',
  build: {
    outDir: 'build',
  },
  css: {
    postcss: {
      plugins: [autoprefixer()],
    },
  },
  plugins: [react()],
  resolve: {
    alias: [{ find: 'src/', replacement: `${path.resolve(__dirname, 'src')}/` }],
  },
  server: {
    host: 'localhost',
    open: !isE2E,
    port,
    strictPort: true,
    proxy: {
      '/api': {
        target: process.env.BACKEND_URL,
        changeOrigin: true,
      },
    },
  },
})
