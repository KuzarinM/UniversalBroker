import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      '^/proxy/.*': {
        target: 'https://localhost:7071',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/proxy/, '')
      }
    }
  }
})