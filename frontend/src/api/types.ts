// The JSON shapes of the REST API, as defined in docs/api-contract.md.

export type Manufacturer = {
  id: number
  name: string
}

/** The category shown with each vehicle. Its image comes from the icons list, looked up by `icon`. */
export type VehicleCategory = {
  id: number
  name: string
  icon: string
}

export type Vehicle = {
  id: number
  ownerName: string
  manufacturer: Manufacturer
  yearOfManufacture: number
  weightKg: number
  category: VehicleCategory
}

export type NewVehicle = {
  ownerName: string
  manufacturerId: number | null
  yearOfManufacture: number | null
  weightKg: number | null
}

export type VehicleSortField = 'ownerName' | 'manufacturer' | 'yearOfManufacture' | 'weightKg'

export type SortDirection = 'asc' | 'desc'

/** A weight category. It includes its minimum and excludes its maximum; a null maximum means no upper limit. */
export type Category = {
  id: number
  name: string
  minWeightKg: number
  maxWeightKg: number | null
  icon: string
  iconSvg: string
}

export type NewCategory = {
  name: string
  minWeightKg: number | null
  maxWeightKg: number | null
  icon: string
}

/** A category's new values. If the range changes, the server moves the neighbouring categories with it. */
export type CategoryChanges = {
  name: string
  icon: string
  minWeightKg: number | null
  maxWeightKg: number | null
}

export type Icon = {
  key: string
  svg: string
}
