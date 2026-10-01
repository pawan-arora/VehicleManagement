import type { Icon } from '../api/types'
import { CategoryIcon } from './CategoryIcon'

type IconSelectProps = {
  icons: Icon[]
  value: string
  onChange: (key: string) => void
}

/** Picks one of the icons from the API, with a preview of the chosen one. */
export function IconSelect({ icons, value, onChange }: IconSelectProps) {
  return (
    <span className="with-icon">
      <select value={value} onChange={(event) => onChange(event.target.value)} aria-label="Icon">
        <option value="">Choose an icon</option>
        {icons.map((icon) => (
          <option key={icon.key} value={icon.key}>
            {icon.key}
          </option>
        ))}
      </select>
      <CategoryIcon svg={icons.find((icon) => icon.key === value)?.svg} label="" />
    </span>
  )
}
