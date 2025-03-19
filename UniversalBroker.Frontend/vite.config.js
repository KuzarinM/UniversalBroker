import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // Проксируем все запросы с таким вот
      '^/proxy/.*': {
        target: 'https://localhost:7071', // Целевой url
        changeOrigin: true, // меняем origin. Возможно оно нам надо
        secure: false, // Отключаем проверки сертификата
        rewrite: (path) => path.replace(/^\/proxy/, '') // Вырезаем слово proxy
      }
    }
  }
})