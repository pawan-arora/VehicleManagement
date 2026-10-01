import type { Icon, SortDirection, Vehicle, VehicleSortField } from '../api/types'
import { CategoryIcon } from '../components/CategoryIcon'
import { formatKg } from '../format'
import { sortOptions } from './sortOptions'

type VehicleTableProps = {
  vehicles: Vehicle[]
  icons: Icon[]
  sortBy: VehicleSortField
  sortDirection: SortDirection
  onSort: (field: VehicleSortField) => void
}

/**
 * The vehicles. Clicking a column heading sorts by it; the sorted column shows ▲ (ascending) or ▼ (descending), and
 * the other sortable columns show ⇅.
 */
export function VehicleTable({ vehicles, icons, sortBy, sortDirection, onSort }: VehicleTableProps) {
  if (vehicles.length === 0) {
    return <p>No vehicles yet. Add one below.</p>
  }

  return (
    <table>
      <thead>
        <tr>
          {sortOptions.map(({ field, label }) => {
            const isSorted = field === sortBy
            return (
              <th key={field} aria-sort={isSorted ? (sortDirection === 'asc' ? 'ascending' : 'descending') : 'none'}>
                <button type="button" className="sort-button" onClick={() => onSort(field)} title={`Sort by ${label}`}>
                  {label} <span aria-hidden="true">{isSorted ? (sortDirection === 'asc' ? '▲' : '▼') : '⇅'}</span>
                </button>
              </th>
            )
          })}
          <th>Category</th>
        </tr>
      </thead>
      <tbody>
        {vehicles.map((vehicle) => (
          <tr key={vehicle.id}>
            <td>{vehicle.ownerName}</td>
            <td>{vehicle.manufacturer.name}</td>
            <td>{vehicle.yearOfManufacture}</td>
            <td>{formatKg(vehicle.weightKg)}</td>
            <td>
              <span className="with-icon">
                <CategoryIcon svg={icons.find((icon) => icon.key === vehicle.category.icon)?.svg} label="" />
                {vehicle.category.name}
              </span>
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
