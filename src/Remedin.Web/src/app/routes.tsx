import { createBrowserRouter } from 'react-router-dom'

import { MedicineDetailPage } from '@/features/medicine-detail/pages/MedicineDetailPage'
import { SearchPage } from '@/features/medicine-search/pages/SearchPage'

import { AppLayout } from './layout/AppLayout'

export const router = createBrowserRouter([
  {
    path: '/',
    Component: AppLayout,
    children: [
      { index: true, Component: SearchPage },
      { path: 'medicamento/:registrationNumber', Component: MedicineDetailPage },
    ],
  },
])
