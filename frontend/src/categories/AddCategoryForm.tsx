import { useState } from 'react'
import type { FormEvent } from 'react'
import { toApiError, type ApiError } from '../api/client'
import type { Category, Icon } from '../api/types'
import { vehicleManagementApi } from '../api/vehicleManagementApi'
import { FieldError, FormError } from '../components/Errors'
import { IconSelect } from '../components/IconSelect'
import { toNumberOrNull } from '../format'

type AddCategoryFormProps = {
  icons: Icon[]
  /** Called with the full list the server returns, since adding a category shortens a neighbour. */
  onAdded: (categories: Category[]) => void
}

const fields = ['Name', 'MinWeightKg', 'MaxWeightKg', 'Icon']

/**
 * Adds a category. It must fit inside one existing category and start or end where that one does; the server shortens
 * that category to make room, and explains in its message if the new range doesn't fit.
 */
export function AddCategoryForm({ icons, onAdded }: AddCategoryFormProps) {
  const [name, setName] = useState('')
  const [minWeight, setMinWeight] = useState('')
  const [maxWeight, setMaxWeight] = useState('')
  const [icon, setIcon] = useState('')
  const [error, setError] = useState<ApiError | null>(null)

  async function save(event: FormEvent) {
    event.preventDefault()
    try {
      onAdded(
        await vehicleManagementApi.addCategory({
          name,
          minWeightKg: toNumberOrNull(minWeight),
          maxWeightKg: toNumberOrNull(maxWeight),
          icon,
        }),
      )
      setName('')
      setMinWeight('')
      setMaxWeight('')
      setIcon('')
      setError(null)
    } catch (caught) {
      setError(toApiError(caught))
    }
  }

  return (
    <form onSubmit={save} noValidate>
      <label>
        Name
        <input value={name} onChange={(event) => setName(event.target.value)} maxLength={50} />
      </label>
      <FieldError error={error} field="Name" />

      <label>
        Minimum weight (kg)
        <input type="number" step="0.01" value={minWeight} onChange={(event) => setMinWeight(event.target.value)} />
      </label>
      <FieldError error={error} field="MinWeightKg" />

      <label>
        Maximum weight (kg)
        <input
          type="number"
          step="0.01"
          value={maxWeight}
          onChange={(event) => setMaxWeight(event.target.value)}
          placeholder="Leave empty for no upper limit"
        />
      </label>
      <FieldError error={error} field="MaxWeightKg" />

      <label>
        Icon
        <IconSelect icons={icons} value={icon} onChange={setIcon} />
      </label>
      <FieldError error={error} field="Icon" />

      <FormError error={error} fields={fields} />
      <button type="submit">Add category</button>
    </form>
  )
}
