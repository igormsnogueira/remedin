import { fileURLToPath, URL } from 'node:url'

import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react()],
  resolve: {
    // Import por caminho absoluto a partir de src. Sem isso, mover um
    // componente de pasta quebra os '../../..' de quem o importa.
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
})
