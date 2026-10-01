import type { VehicleSortField } from '../api/types'

/** The fields the vehicle list can be sorted by (see docs/api-contract.md), with their labels. */
export const sortOptions: { field: VehicleSortField; label: string }[] = [
  { field: 'ownerName', label: "Owner's name" },
  { field: 'manufacturer', label: 'Manufacturer' },
  { field: 'yearOfManufacture', label: 'Year' },
  { field: 'weightKg', label: 'Weight' },
]
