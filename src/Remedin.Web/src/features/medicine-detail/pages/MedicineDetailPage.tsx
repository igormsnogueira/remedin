import { Link, useParams } from 'react-router-dom'

import { useSelectedState } from '@/app/providers/useSelectedState'
import { AlternativesSection } from '@/features/medicine-alternatives/components/AlternativesSection'
import { ApiLoading } from '@/shared/api/ApiLoading'
import { ApiError } from '@/shared/api/client'
import { useQuery } from '@/shared/api/useQuery'
import type { MedicineDetail } from '@/shared/api/types'
import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'

import { MedicineAside } from '../components/MedicineAside'
import { MedicineFacts } from '../components/MedicineFacts'
import { MedicineHeader } from '../components/MedicineHeader'
import { PackagesSection } from '../components/PackagesSection'
import { fetchMedicine } from '../api/fetchMedicine'
import { groupPackages } from '../model/packages'
import { useSelectedPackage } from '../model/useSelectedPackage'

import styles from './MedicineDetailPage.module.css'

export function MedicineDetailPage() {
  const { registrationNumber } = useParams()
  const { state } = useSelectedState()

  const medicine = useQuery<MedicineDetail>(
    registrationNumber ? (signal) => fetchMedicine(registrationNumber, state, signal) : null,
    [registrationNumber, state],
  )

  useDocumentTitle(medicine.status === 'success' ? medicine.data.name : null)

  const presentations = medicine.status === 'success' ? medicine.data.presentations : []
  const pharmacy = groupPackages(presentations, 'pharmacy')
  const institutional = groupPackages(presentations, 'institutional')
  const hospital = groupPackages(presentations, 'hospital')
  const { selected, select } = useSelectedPackage(pharmacy)

  if (medicine.status === 'loading' || medicine.status === 'idle') {
    return <ApiLoading label="Carregando o medicamento" shape="detail" />
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
    <article className={styles.layout}>
      <div className={styles.head}>
        <MedicineHeader medicine={medicine.data} />
        <MedicineFacts medicine={medicine.data} packages={pharmacy} selected={selected} />
      </div>

      <MedicineAside medicine={medicine.data} />

      <div className={styles.body}>
        <PackagesSection
          pharmacy={pharmacy}
          institutional={institutional}
          hospital={hospital}
          selectedKey={selected?.key ?? null}
          onSelect={select}
        />
        <AlternativesSection
          registrationNumber={medicine.data.registrationNumber}
          reference={{
            milligrams: selected?.milligrams ?? null,
            strength: selected?.strength ?? null,
            form: selected ? (selected.units ? 'counted' : 'measured') : null,
            release: selected?.release ?? null,
            perUnit: selected?.perUnit ?? null,
            unitName: selected?.unitLabel ?? null,
          }}
        />
      </div>
    </article>
  )
}
