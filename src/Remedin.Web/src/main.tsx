// Primeiro import de propósito: define a ordem das camadas de CSS.
import './shared/styles/index.css'

import '@fontsource/atkinson-hyperlegible/400.css'
import '@fontsource/atkinson-hyperlegible/700.css'

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'

import { SelectedStateProvider } from './app/providers/SelectedStateProvider'
import { router } from './app/routes'
import { wakeUpApi } from './shared/api/client'

// O servidor gratuito dorme sem uso; começar a acordá-lo ao abrir encurta a primeira espera.
wakeUpApi()

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
