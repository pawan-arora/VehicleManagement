import { useEffect, useState } from 'react'
import { toApiError, type ApiError } from '../api/client'
import type { Category, Icon } from '../api/types'
import { vehicleManagementApi } from '../api/vehicleManagementApi'
import { FormError } from '../components/Errors'
import { AddCategoryForm } from './AddCategoryForm'
import { CategoryRow } from './CategoryRow'

type CategoriesTabProps = {
  icons: Icon[]
}

/**
 * The categories, lightest first, with a form to add one. Adding or deleting a category can move a neighbour's
 * boundary, so every change returns the full list from the server, which replaces the one shown.
 */
export function CategoriesTab({ icons }: CategoriesTabProps) {
  const [categories, setCategories] = useState<Category[]>([])
  const [error, setError] = useState<ApiError | null>(null)

  useEffect(() => {
    vehicleManagementApi
      .getCategories()
      .then(setCategories)
      .catch((caught) => setError(toApiError(caught)))
  }, [])

  return (
    <section>
      <h2>Categories</h2>
      <p className="hint">
        A category includes its minimum weight and excludes its maximum, so 500 kg belongs to the category that starts at
        500 kg.
      </p>
      <FormError error={error} fields={[]} />
      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Weight range</th>
            <th>Icon</th>
            <th>
              <span className="visually-hidden">Actions</span>
            </th>
          </tr>
        </thead>
        <tbody>
          {categories.map((category) => (
            <CategoryRow key={category.id} category={category} icons={icons} onChanged={setCategories} />
          ))}
        </tbody>
      </table>

      <h2>Add a category</h2>
      <AddCategoryForm icons={icons} onAdded={setCategories} />
    </section>
  )
}
