import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    // En desarrollo, evita CORS reenviando las llamadas al backend local.
    proxy: {
      '/api': 'http://localhost:8080',
    },
  },
});
