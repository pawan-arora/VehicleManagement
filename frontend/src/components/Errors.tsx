import type { ApiError } from '../api/client'

type FieldErrorProps = {
  error: ApiError | null
  /** The server's field name, such as `WeightKg` (see docs/api-contract.md). */
  field: string
}

/** The server's messages for one field, shown under that field. */
export function FieldError({ error, field }: FieldErrorProps) {
  const messages = error?.fieldErrors[field] ?? []

  return messages.map((message) => (
    <p key={message} className="error">
      {message}
    </p>
  ))
}

type FormErrorProps = {
  error: ApiError | null
  /** The fields that show their own messages with FieldError. Everything else is shown here. */
  fields: string[]
}

/**
 * The messages that aren't shown next to a field: rules about several records (such as a gap between categories),
 * or the error itself when it has no field messages (such as "Could not reach the server").
 */
export function FormError({ error, fields }: FormErrorProps) {
  if (!error) {
    return null
  }

  const otherEntries = Object.entries(error.fieldErrors).filter(([field]) => !fields.includes(field))
  const messages = Object.keys(error.fieldErrors).length === 0 ? [error.message] : otherEntries.flatMap(([, list]) => list)

  return messages.map((message) => (
    <p key={message} className="error" role="alert">
      {message}
    </p>
  ))
}
