import { useEffect, useState } from 'react'
import { toApiError, type ApiError } from '../api/client'
import type { Icon, SortDirection, Vehicle, VehicleSortField } from '../api/types'
import { vehicleManagementApi } from '../api/vehicleManagementApi'
import { FormError } from '../components/Errors'
import { AddVehicleForm } from './AddVehicleForm'
import { SortControls } from './SortControls'
import { VehicleTable } from './VehicleTable'

type VehiclesTabProps = {
  icons: Icon[]
}

/** The vehicle list, sorted on the server, and the form to add a vehicle. */
export function VehiclesTab({ icons }: VehiclesTabProps) {
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [sortBy, setSortBy] = useState<VehicleSortField>('ownerName')
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc')
  const [error, setError] = useState<ApiError | null>(null)
  // Adding a vehicle bumps this, which reloads the list below.
  const [reloadCount, setReloadCount] = useState(0)

  // Loads the vehicles when the tab opens, when the sort order changes, and after a vehicle is added.
  useEffect(() => {
    vehicleManagementApi
      .getVehicles(sortBy, sortDirection)
      .then((loaded) => {
        setVehicles(loaded)
        setError(null)
      })
      .catch((caught) => setError(toApiError(caught)))
  }, [sortBy, sortDirection, reloadCount])

  function sort(field: VehicleSortField, direction: SortDirection) {
    setSortBy(field)
    setSortDirection(direction)
  }

  /** Clicking the column that's already sorted reverses it; clicking another column sorts by it, ascending. */
  function sortByHeading(field: VehicleSortField) {
    sort(field, field === sortBy && sortDirection === 'asc' ? 'desc' : 'asc')
  }

  return (
    <section>
      <h2>Vehicles</h2>
      <FormError error={error} fields={[]} />
      <SortControls sortBy={sortBy} sortDirection={sortDirection} onChange={sort} />
      <VehicleTable
        vehicles={vehicles}
        icons={icons}
        sortBy={sortBy}
        sortDirection={sortDirection}
        onSort={sortByHeading}
      />

      <h2>Add a vehicle</h2>
      <AddVehicleForm onAdded={() => setReloadCount((count) => count + 1)} />
    </section>
  )
}
