import { request } from './client'
import type {
  Category,
  CategoryChanges,
  Icon,
  Manufacturer,
  NewCategory,
  NewVehicle,
  SortDirection,
  Vehicle,
  VehicleSortField,
} from './types'

// One function per REST endpoint in docs/api-contract.md. The rest of the app only calls these.

export const vehicleManagementApi = {
  getVehicles: (sortBy: VehicleSortField, sortDirection: SortDirection) =>
    request<Vehicle[]>('GET', `/api/vehicles?sortBy=${sortBy}&sortDirection=${sortDirection}`),

  addVehicle: (vehicle: NewVehicle) => request<Vehicle>('POST', '/api/vehicles', vehicle),

  getManufacturers: () => request<Manufacturer[]>('GET', '/api/manufacturers'),

  getCategories: () => request<Category[]>('GET', '/api/categories/all'),

  findCategoryByWeight: (weightKg: number) =>
    request<Category>('GET', `/api/categories/findByWeight?weightKg=${weightKg}`),

  // Adding, editing or deleting a category can move a neighbour's boundary, so each returns the full list.
  addCategory: (category: NewCategory) => request<Category[]>('POST', '/api/categories', category),

  updateCategory: (id: number, changes: CategoryChanges) => request<Category[]>('PUT', `/api/categories/${id}`, changes),

  deleteCategory: (id: number) => request<Category[]>('DELETE', `/api/categories/${id}`),

  getIcons: () => request<Icon[]>('GET', '/api/icons'),
}
