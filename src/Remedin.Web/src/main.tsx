import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'

import { SelectedStateProvider } from './app/providers/SelectedStateProvider'
import { router } from './app/routes'

import './shared/styles/index.css'

const container = document.getElementById('root')

if (!container) {
  throw new Error('Elemento #root não encontrado no index.html.')
}

createRoot(container).render(
  <StrictMode>
    <SelectedStateProvider>
      <RouterProvider router={router} />
    </SelectedStateProvider>
  </StrictMode>,
)
