/**
 * Custom fetch with retry logic, authentication, and OpenTelemetry tracing.
 * Used as a mutator by Orval-generated API client code.
 */

import { getTracer } from '../services/telemetry'

// ============================================================================
// Types
// ============================================================================

export interface RetryConfig {
  maxRetries: number
  retryDelay: number
  retryDelayMultiplier: number
  retryableStatusCodes: number[]
}

export interface ApiClientConfig {
  baseUrl?: string
  timeout?: number
  retry?: Partial<RetryConfig>
  tokenProvider?: () => Promise<string | null>
  onUnauthorized?: (error: ApiError) => void
  onError?: (error: ApiError) => void
}

// ============================================================================
// Configuration
// ============================================================================

const DEFAULT_RETRY_CONFIG: RetryConfig = {
  maxRetries: 3,
  retryDelay: 1000,
  retryDelayMultiplier: 2,
  retryableStatusCodes: [408, 429, 500, 502, 503, 504],
}

let config: Required<Omit<ApiClientConfig, 'retry'>> & { retry: RetryConfig } = {
  baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:8080/api/v1',
  timeout: 30000,
  retry: { ...DEFAULT_RETRY_CONFIG },
  tokenProvider: async () => null,
  onUnauthorized: () => {},
  onError: () => {},
}

/**
 * Configure the API client with authentication, retry logic, and callbacks.
 * Call this once during app initialization.
 */
export function configureApi(options: ApiClientConfig): void {
  config = {
    baseUrl: options.baseUrl ?? config.baseUrl,
    timeout: options.timeout ?? config.timeout,
    retry: { ...config.retry, ...options.retry },
    tokenProvider: options.tokenProvider ?? config.tokenProvider,
    onUnauthorized: options.onUnauthorized ?? config.onUnauthorized,
    onError: options.onError ?? config.onError,
  }
}

/**
 * Update only the token provider (useful for React hooks)
 */
export function setTokenProvider(tokenProvider: () => Promise<string | null>): void {
  config.tokenProvider = tokenProvider
}

/**
 * Get the configured base URL for API requests.
 */
export function getBaseUrl(): string {
  return config.baseUrl
}

// ============================================================================
// API Error Class
// ============================================================================

/**
 * Custom error class for API errors with ProblemDetails support
 */
export class ApiError extends Error {
  statusCode: number
  errors: string[]
  response: unknown
  timestamp: string
  problemDetails?: {
    type?: string
    title?: string
    status?: number
    detail?: string
    instance?: string
  }

  constructor(
    message: string,
    statusCode: number,
    errors: string[] = [],
    response: unknown = null,
  ) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
    this.errors = errors
    this.response = response
    this.timestamp = new Date().toISOString()

    // Extract ProblemDetails if available
    if (response && typeof response === 'object') {
      const data = response as Record<string, unknown>
      this.problemDetails = {
        type: data.type as string | undefined,
        title: data.title as string | undefined,
        status: data.status as number | undefined,
        detail: data.detail as string | undefined,
        instance: data.instance as string | undefined,
      }
    }
  }

  isClientError(): boolean {
    return this.statusCode >= 400 && this.statusCode < 500
  }

  isServerError(): boolean {
    return this.statusCode >= 500
  }

  isUnauthorized(): boolean {
    return this.statusCode === 401
  }

  isForbidden(): boolean {
    return this.statusCode === 403
  }

  isNotFound(): boolean {
    return this.statusCode === 404
  }

  isBadRequest(): boolean {
    return this.statusCode === 400
  }

  /**
   * Get a user-friendly error message for display in the UI
   */
  getUserFriendlyMessage(): string {
    if (this.problemDetails?.detail) {
      return this.problemDetails.detail
    }
    if (this.problemDetails?.title) {
      return this.problemDetails.title
    }
    return this.message
  }

  /**
   * Get all error messages including validation errors
   */
  getAllErrorMessages(): string[] {
    const messages = [this.getUserFriendlyMessage()]
    if (this.errors && this.errors.length > 0) {
      messages.push(...this.errors)
    }
    return messages
  }
}

// ============================================================================
// Custom Fetch Implementation
// ============================================================================

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

/**
 * Custom fetch function with retry logic, authentication, and OpenTelemetry tracing.
 * Used as a mutator by Orval-generated code.
 */
export async function customFetch<T>(url: string, options: RequestInit = {}): Promise<T> {
  // Build full URL - strip /api/v1 prefix if present since baseUrl includes it
  const path = url.startsWith('/api/v1') ? url.slice(7) : url
  const fullUrl = path.startsWith('http') ? path : `${config.baseUrl}${path}`

  // Resolve relative URLs against the current origin (e.g. when baseUrl is "/api/v1")
  const requestUrl = fullUrl.startsWith('http')
    ? fullUrl
    : typeof window !== 'undefined'
      ? new URL(fullUrl, window.location.origin).toString()
      : new URL(fullUrl, 'http://localhost').toString()

  // Create OpenTelemetry span
  const tracer = getTracer()
  const parsedUrl = new URL(requestUrl)
  const span = tracer.startSpan(`HTTP ${options.method || 'GET'}`, {
    attributes: {
      'http.method': options.method || 'GET',
      'http.url': requestUrl,
      'http.scheme': parsedUrl.protocol.replace(':', ''),
      'http.host': parsedUrl.hostname,
      'http.target': parsedUrl.pathname + parsedUrl.search,
      'service.name': 'vertical-slice-client',
    },
  })

  let lastError: ApiError | null = null

  try {
    for (let attempt = 0; attempt <= config.retry.maxRetries; attempt++) {
      try {
        // Get auth token
        const headers = new Headers(options.headers)
        if (!headers.has('Content-Type') && options.body && !(options.body instanceof FormData)) {
          headers.set('Content-Type', 'application/json')
        }

        try {
          const token = await config.tokenProvider()
          if (token) {
            headers.set('Authorization', `Bearer ${token}`)
          }
        } catch (error) {
          console.warn('Failed to get auth token:', error)
        }

        // Create abort controller for timeout
        const controller = new AbortController()
        const timeoutId = setTimeout(() => controller.abort(), config.timeout)

        try {
          const response = await fetch(requestUrl, {
            ...options,
            headers,
            signal: options.signal || controller.signal,
          })

          clearTimeout(timeoutId)

          // Handle error responses
          if (!response.ok) {
            const errorData = await response.json().catch(() => ({}))

            // Parse ProblemDetails structure
            const problemDetails = errorData as {
              type?: string
              title?: string
              status?: number
              detail?: string
              instance?: string
              errors?: string[]
            }

            const errorMessage =
              problemDetails.detail ||
              problemDetails.title ||
              errorData.message ||
              `HTTP ${response.status} error`

            const errors =
              problemDetails.errors ||
              errorData.errors ||
              (errorData.validationErrors ? Object.values(errorData.validationErrors).flat() : [])

            const error = new ApiError(errorMessage, response.status, errors as string[], errorData)

            // Call error handlers
            if (error.isUnauthorized()) {
              config.onUnauthorized(error)
            }
            config.onError(error)

            // Check if we should retry
            if (
              attempt < config.retry.maxRetries &&
              config.retry.retryableStatusCodes.includes(response.status)
            ) {
              lastError = error
              const delay =
                config.retry.retryDelay * Math.pow(config.retry.retryDelayMultiplier, attempt)
              await sleep(delay)
              continue
            }

            throw error
          }

          // Add response attributes to span
          span.setAttributes({
            'http.status_code': response.status,
            'http.response.size': response.headers.get('content-length') || 'unknown',
          })

          // Parse response
          const contentType = response.headers.get('content-type')

          // Handle 204 No Content
          if (response.status === 204) {
            span.setStatus({ code: 1 })
            // Return Orval-expected format: { data, status, headers }
            return { data: undefined, status: 204, headers: response.headers } as T
          }

          if (contentType?.includes('application/json')) {
            const result = await response.json()
            span.setStatus({ code: 1 })
            // Return Orval-expected format: { data, status, headers }
            return { data: result, status: response.status, headers: response.headers } as T
          }

          const textResult = await response.text()
          span.setStatus({ code: 1 })
          // Return Orval-expected format: { data, status, headers }
          return { data: textResult, status: response.status, headers: response.headers } as T
        } finally {
          clearTimeout(timeoutId)
        }
      } catch (error) {
        // Re-throw ApiErrors
        if (error instanceof ApiError) {
          throw error
        }

        // Retry on network errors
        if (attempt < config.retry.maxRetries) {
          lastError = new ApiError((error as Error).message || 'Network request failed', 0, [
            (error as Error).message,
          ])
          const delay =
            config.retry.retryDelay * Math.pow(config.retry.retryDelayMultiplier, attempt)
          await sleep(delay)
          continue
        }

        throw new ApiError((error as Error).message || 'Network request failed', 0, [
          (error as Error).message,
        ])
      }
    }

    // All retries exhausted
    throw lastError || new ApiError('Request failed after all retries', 0)
  } catch (error) {
    span.setStatus({
      code: 2,
      message: error instanceof Error ? error.message : 'Unknown error',
    })
    if (error instanceof Error) {
      span.recordException(error)
    }
    throw error
  } finally {
    span.end()
  }
}

export default customFetch
