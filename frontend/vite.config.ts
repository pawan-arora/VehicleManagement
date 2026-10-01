import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    // The app calls the REST API with relative paths such as /api/vehicles. In development, Vite forwards them to
    // the backend, so the app code never contains the backend's address and the backend knows nothing about the app.
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})
