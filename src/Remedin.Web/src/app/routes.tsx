import { createBrowserRouter } from 'react-router-dom'

import { MedicineDetailPage } from '@/features/medicine-detail/pages/MedicineDetailPage'
import { SearchPage } from '@/features/medicine-search/pages/SearchPage'

import { AppLayout } from './layout/AppLayout'
import { ErrorPage } from './layout/ErrorPage'
import { NotFoundPage } from './layout/NotFoundPage'

export const router = createBrowserRouter([
  {
    path: '/',
    Component: AppLayout,
    // No pai, cobre falha do próprio layout. Nos filhos, mantém cabeçalho e
    // rodapé na tela quando o erro é de uma página só.
    ErrorBoundary: ErrorPage,
    children: [
      { index: true, Component: SearchPage, ErrorBoundary: ErrorPage },
      {
        path: 'medicamento/:registrationNumber',
        Component: MedicineDetailPage,
        ErrorBoundary: ErrorPage,
      },
      { path: '*', Component: NotFoundPage },
    ],
  },
])
