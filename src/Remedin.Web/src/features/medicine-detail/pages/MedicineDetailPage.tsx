import { Link, useParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { AlternativesSection } from '@/features/medicine-alternatives/components/AlternativesSection'
import { ApiError } from '@/shared/api/client'
import { useQuery } from '@/shared/api/useQuery'
import type { MedicineDetail } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'
import { Spinner } from '@/shared/ui/Spinner/Spinner'

import { MedicineHeader } from '../components/MedicineHeader'
import { PrescriptionSection } from '../components/PrescriptionSection'
import { PriceSection } from '../components/PriceSection'
import { PurposeSection } from '../components/PurposeSection'
import { fetchMedicine } from '../api/fetchMedicine'

import styles from './MedicineDetailPage.module.css'

export function MedicineDetailPage() {
  const { registrationNumber } = useParams()
  const { state } = useSelectedState()

  const medicine = useQuery<MedicineDetail>(
    registrationNumber ? (signal) => fetchMedicine(registrationNumber, state, signal) : null,
    [registrationNumber, state],
  )

  useDocumentTitle(medicine.status === 'success' ? medicine.data.name : null)

  if (medicine.status === 'loading' || medicine.status === 'idle') {
    return <Spinner label="Carregando o medicamento" />
  }

  if (medicine.status === 'error') {
    const notFound = medicine.error instanceof ApiError && medicine.error.isNotFound

    return (
      <div className={styles.page}>
        <Notice tone="danger">
          {notFound ? 'Medicamento não encontrado.' : medicine.error.message}
        </Notice>
        <Link to="/">Voltar para a busca</Link>
      </div>
    )
  }

  return (
    <article className={styles.page}>
      <MedicineHeader medicine={medicine.data} />
      <PurposeSection medicine={medicine.data} />
      <PrescriptionSection medicine={medicine.data} />
      <PriceSection medicine={medicine.data} />
      <AlternativesSection registrationNumber={medicine.data.registrationNumber} />

      <Link to="/">Voltar para a busca</Link>
    </article>
  )
}
