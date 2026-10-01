type CategoryIconProps = {
  /** The icon's SVG markup, from the API. */
  svg: string | undefined
  /** Text for screen readers, such as the category name. */
  label: string
}

/**
 * Shows an icon's SVG through an <img>. An image can't run scripts, so this is safe even though the SVG comes from
 * the server; inserting the markup into the page directly would not be.
 */
export function CategoryIcon({ svg, label }: CategoryIconProps) {
  if (!svg) {
    return null
  }

  return (
    <img
      className="category-icon"
      src={`data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`}
      alt={label}
      width={24}
      height={24}
    />
  )
}
