import type { SortDirection, VehicleSortField } from '../api/types'
import { sortOptions } from './sortOptions'

type SortControlsProps = {
  sortBy: VehicleSortField
  sortDirection: SortDirection
  onChange: (sortBy: VehicleSortField, sortDirection: SortDirection) => void
}

/** "Sort by [field] [direction]" above the vehicle list, so the current order is always visible. */
export function SortControls({ sortBy, sortDirection, onChange }: SortControlsProps) {
  return (
    <div className="sort-controls">
      <label>
        Sort by
        <select aria-label="Sort by" value={sortBy} onChange={(event) => onChange(event.target.value as VehicleSortField, sortDirection)}>
          {sortOptions.map(({ field, label }) => (
            <option key={field} value={field}>
              {label}
            </option>
          ))}
        </select>
      </label>
      <label>
        Order
        <select aria-label="Order" value={sortDirection} onChange={(event) => onChange(sortBy, event.target.value as SortDirection)}>
          <option value="asc">Ascending (A–Z, oldest, lightest first)</option>
          <option value="desc">Descending (Z–A, newest, heaviest first)</option>
        </select>
      </label>
    </div>
  )
}
