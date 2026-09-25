import { useSelectedState } from '@/app/providers/useSelectedState'
import { useQuery } from '@/shared/api/useQuery'
import type { StateOption } from '@/shared/api/types'
import { SelectField } from '@/shared/ui/Field/SelectField'

import { fetchStates } from '../api/fetchStates'

/**
 * Seletor do estado do usuário.
 *
 * Enquanto a lista não chega, mostra só a sigla já escolhida: um seletor vazio
 * pareceria defeito, e a escolha anterior continua válida.
 */
export function StateSelect() {
  const { state, setState } = useSelectedState()
  const states = useQuery<StateOption[]>(fetchStates, [])

  const options = states.status === 'success' ? states.data : []

  return (
    <SelectField
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
