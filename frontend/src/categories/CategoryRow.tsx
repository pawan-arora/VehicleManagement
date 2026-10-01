import { useState } from 'react'
import { toApiError, type ApiError } from '../api/client'
import type { Category, Icon } from '../api/types'
import { vehicleManagementApi } from '../api/vehicleManagementApi'
import { CategoryIcon } from '../components/CategoryIcon'
import { FieldError, FormError } from '../components/Errors'
import { IconSelect } from '../components/IconSelect'
import { formatRange, toNumberOrNull } from '../format'

type CategoryRowProps = {
  category: Category
  icons: Icon[]
  /** Called with the full list the server returns after an edit or delete. */
  onChanged: (categories: Category[]) => void
}

/**
 * One category. Its name, icon and weight range can be edited. If the range changes, the server moves the neighbouring
 * categories so the ranges keep touching, and returns the full list.
 */
export function CategoryRow({ category, icons, onChanged }: CategoryRowProps) {
  const [isEditing, setIsEditing] = useState(false)
  const [name, setName] = useState(category.name)
  const [icon, setIcon] = useState(category.icon)
  const [minWeight, setMinWeight] = useState(String(category.minWeightKg))
  const [maxWeight, setMaxWeight] = useState(category.maxWeightKg === null ? '' : String(category.maxWeightKg))
  const [error, setError] = useState<ApiError | null>(null)

  function startEditing() {
    setName(category.name)
    setIcon(category.icon)
    setMinWeight(String(category.minWeightKg))
    setMaxWeight(category.maxWeightKg === null ? '' : String(category.maxWeightKg))
    setError(null)
    setIsEditing(true)
  }

  async function save() {
    try {
      onChanged(
        await vehicleManagementApi.updateCategory(category.id, {
          name,
          icon,
          minWeightKg: toNumberOrNull(minWeight),
          maxWeightKg: toNumberOrNull(maxWeight),
        }),
      )
      setIsEditing(false)
    } catch (caught) {
      setError(toApiError(caught))
    }
  }

  async function remove() {
    const confirmed = window.confirm(
      `Delete '${category.name}'? The next heavier category takes over its weight range ` +
        `(or the next lighter one, if this is the heaviest).`,
    )
    if (!confirmed) {
      return
    }

    try {
      onChanged(await vehicleManagementApi.deleteCategory(category.id))
    } catch (caught) {
      setError(toApiError(caught))
    }
  }

  const range = formatRange(category.minWeightKg, category.maxWeightKg)

  if (isEditing) {
    return (
      <tr>
        <td>
          <input value={name} onChange={(event) => setName(event.target.value)} aria-label="Name" maxLength={50} />
          <FieldError error={error} field="Name" />
        </td>
        <td>
          <div className="range-inputs">
            <input
              type="number"
              step="0.01"
              value={minWeight}
              onChange={(event) => setMinWeight(event.target.value)}
              aria-label="Minimum weight (kg)"
            />
            to
            <input
              type="number"
              step="0.01"
              value={maxWeight}
              onChange={(event) => setMaxWeight(event.target.value)}
              aria-label="Maximum weight (kg)"
              placeholder="No limit"
            />
          </div>
          <FieldError error={error} field="MinWeightKg" />
          <FieldError error={error} field="MaxWeightKg" />
        </td>
        <td>
          <IconSelect icons={icons} value={icon} onChange={setIcon} />
          <FieldError error={error} field="Icon" />
        </td>
        <td>
          <button type="button" onClick={save}>
            Save
          </button>
          <button type="button" onClick={() => setIsEditing(false)}>
            Cancel
          </button>
          <FormError error={error} fields={['Name', 'Icon', 'MinWeightKg', 'MaxWeightKg']} />
        </td>
      </tr>
    )
  }

  return (
    <tr>
      <td>{category.name}</td>
      <td>{range}</td>
      <td>
        <CategoryIcon svg={category.iconSvg} label={category.icon} />
      </td>
      <td>
        <button type="button" onClick={startEditing}>
          Edit
        </button>
        <button type="button" onClick={remove}>
          Delete
        </button>
        <FormError error={error} fields={[]} />
      </td>
    </tr>
  )
}
