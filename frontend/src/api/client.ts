/**
 * An API call that failed. For a 400, `fieldErrors` holds the server's messages by field name (such as `WeightKg`);
 * messages that aren't about one field are under a general key (such as `Categories`).
 */
export class ApiError extends Error {
  status: number
  fieldErrors: Record<string, string[]>

  constructor(message: string, status: number, fieldErrors: Record<string, string[]> = {}) {
    super(message)
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

/** Any caught error as an ApiError, so every screen can show it the same way. */
export function toApiError(error: unknown): ApiError {
  return error instanceof ApiError ? error : new ApiError('Something went wrong. Please try again.', 0)
}

/** The RFC 7807 problem details body the API sends with every error (see docs/api-contract.md). */
type ProblemDetails = {
  title?: string
  errors?: Record<string, string[]>
}

/**
 * Calls the REST API and returns the JSON response. Paths are relative (such as `/api/vehicles`): in development,
 * Vite forwards them to the backend (see vite.config.ts). Throws an ApiError when the call fails.
 */
export async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  let response: Response
  try {
    response = await fetch(path, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError('Could not reach the server. Is the API running?', 0)
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails
    throw new ApiError(problem.title ?? `The request failed (${response.status}).`, response.status, problem.errors)
  }

  return (await response.json()) as T
}
