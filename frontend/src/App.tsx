import { useEffect, useState } from 'react'
import type { Icon } from './api/types'
import { vehicleManagementApi } from './api/vehicleManagementApi'
import { CategoriesTab } from './categories/CategoriesTab'
import { VehiclesTab } from './vehicles/VehiclesTab'

type Tab = 'vehicles' | 'categories'

/**
 * The page: a Vehicles tab and a Categories tab. Switching tabs reloads that tab's data, so after changing the
 * categories, the Vehicles tab shows each vehicle's new category.
 */
function App() {
  const [tab, setTab] = useState<Tab>('vehicles')
  const [icons, setIcons] = useState<Icon[]>([])

  // Both tabs show icons, so they're loaded once here.
  useEffect(() => {
    vehicleManagementApi
      .getIcons()
      .then(setIcons)
      .catch(() => setIcons([]))
  }, [])

  return (
    <main>
      <h1>Vehicle Management</h1>

      <div role="tablist" className="tabs">
        <button type="button" role="tab" aria-selected={tab === 'vehicles'} onClick={() => setTab('vehicles')}>
          Vehicles
        </button>
        <button type="button" role="tab" aria-selected={tab === 'categories'} onClick={() => setTab('categories')}>
          Categories
        </button>
      </div>

      <div role="tabpanel">{tab === 'vehicles' ? <VehiclesTab icons={icons} /> : <CategoriesTab icons={icons} />}</div>
    </main>
  )
}

export default App
