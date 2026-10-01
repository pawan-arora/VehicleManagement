/** A weight for display, such as "1,850.75 kg". */
export function formatKg(weightKg: number): string {
  return `${weightKg.toLocaleString('en-NZ', { maximumFractionDigits: 2 })} kg`
}

/** A category's range for display: "500 kg to 2,500 kg", or "2,500 kg and above" when it has no upper limit. */
export function formatRange(minWeightKg: number, maxWeightKg: number | null): string {
  return maxWeightKg === null
    ? `${formatKg(minWeightKg)} and above`
    : `${formatKg(minWeightKg)} to ${formatKg(maxWeightKg)}`
}

/** A number typed into a form field, or null when the field is empty. */
export function toNumberOrNull(text: string): number | null {
  return text.trim() === '' ? null : Number(text)
}
