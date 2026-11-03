// Primeiro import do arquivo, de propósito: é aqui que a ordem das camadas de
// cascata é declarada, e a ordem vale a partir da primeira vez que cada camada
// aparece. Se um CSS Module de componente for injetado antes, ele cria a camada
// "components" primeiro, e "base" acaba passando a vencê-la.
import './shared/styles/index.css'

// A fonte vem empacotada com o site, e não de um servidor de fontes: assim a
// política de segurança continua aceitando só a própria origem. Só os dois
// pesos que existem nela.
import '@fontsource/atkinson-hyperlegible/400.css'
import '@fontsource/atkinson-hyperlegible/700.css'

import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'

import { SelectedStateProvider } from './app/providers/SelectedStateProvider'
import { router } from './app/routes'

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
