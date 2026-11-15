import { useSelectedState } from '@/app/providers/useSelectedState'
import { useQuery } from '@/shared/api/useQuery'
import type { StateOption } from '@/shared/api/types'
import { SelectField } from '@/shared/ui/Field/SelectField'

import { fetchStates } from '../api/fetchStates'

export function StateSelect({ className }: { className?: string }) {
  const { state, setState } = useSelectedState()
  const states = useQuery<StateOption[]>(fetchStates, [])

  const options = states.status === 'success' ? states.data : []

  return (
    <SelectField
      className={className}
      label="Estado"
      value={state}
      onChange={(event) => setState(event.target.value)}
      disabled={options.length === 0}
    >
      {options.length === 0 ? (
        <option value={state}>{state}</option>
      ) : (
        options.map((option) => (
          <option key={option.code} value={option.code}>
            {option.name}
          </option>
        ))
      )}
    </SelectField>
  )
}
