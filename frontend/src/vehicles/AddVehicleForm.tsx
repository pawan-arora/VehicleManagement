import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { toApiError, type ApiError } from '../api/client'
import type { Category, Manufacturer } from '../api/types'
import { vehicleManagementApi } from '../api/vehicleManagementApi'
import { CategoryIcon } from '../components/CategoryIcon'
import { FieldError, FormError } from '../components/Errors'
import { toNumberOrNull } from '../format'

type AddVehicleFormProps = {
  onAdded: () => void
}

const fields = ['OwnerName', 'ManufacturerId', 'YearOfManufacture', 'WeightKg']

/**
 * Adds a vehicle. After the weight is entered, it shows which category the vehicle will be in. The server checks
 * every rule when the form is saved, and its messages appear next to the fields.
 */
export function AddVehicleForm({ onAdded }: AddVehicleFormProps) {
  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([])
  const [ownerName, setOwnerName] = useState('')
  const [manufacturerId, setManufacturerId] = useState('')
  const [year, setYear] = useState('')
  const [weight, setWeight] = useState('')
  const [category, setCategory] = useState<Category | null>(null)
  const [error, setError] = useState<ApiError | null>(null)

  useEffect(() => {
    vehicleManagementApi
      .getManufacturers()
      .then(setManufacturers)
      .catch((caught) => setError(toApiError(caught)))
  }, [])

  /** When the user leaves the weight box, shows which category that weight belongs to. */
  async function showCategory() {
    const weightKg = toNumberOrNull(weight)
    if (weightKg === null || weightKg <= 0) {
      setCategory(null)
      return
    }

    try {
      setCategory(await vehicleManagementApi.findCategoryByWeight(weightKg))
    } catch {
      setCategory(null)
    }
  }

  async function save(event: FormEvent) {
    event.preventDefault()
    try {
      await vehicleManagementApi.addVehicle({
        ownerName,
        manufacturerId: toNumberOrNull(manufacturerId),
        yearOfManufacture: toNumberOrNull(year),
        weightKg: toNumberOrNull(weight),
      })
      setOwnerName('')
      setManufacturerId('')
      setYear('')
      setWeight('')
      setCategory(null)
      setError(null)
      onAdded()
    } catch (caught) {
      setError(toApiError(caught))
    }
  }

  return (
    <form onSubmit={save} noValidate>
      <label>
        Owner's name
        <input value={ownerName} onChange={(event) => setOwnerName(event.target.value)} maxLength={100} />
      </label>
      <FieldError error={error} field="OwnerName" />

      <label>
        Manufacturer
        <select value={manufacturerId} onChange={(event) => setManufacturerId(event.target.value)}>
          <option value="">Choose a manufacturer</option>
          {manufacturers.map((manufacturer) => (
            <option key={manufacturer.id} value={manufacturer.id}>
              {manufacturer.name}
            </option>
          ))}
        </select>
      </label>
      <FieldError error={error} field="ManufacturerId" />

      <label>
        Year of manufacture
        <input type="number" value={year} onChange={(event) => setYear(event.target.value)} />
      </label>
      <FieldError error={error} field="YearOfManufacture" />

      <label>
        Weight (kg)
        <input
          type="number"
          step="0.01"
          value={weight}
          onChange={(event) => setWeight(event.target.value)}
          onBlur={showCategory}
        />
      </label>
      <FieldError error={error} field="WeightKg" />
      {category && (
        <p className="with-icon">
          Category: <CategoryIcon svg={category.iconSvg} label="" /> {category.name}
        </p>
      )}

      <FormError error={error} fields={fields} />
      <button type="submit">Add vehicle</button>
    </form>
  )
}
