
// Configuration and error handling
export {
  configureApi,
  getBaseUrl,
  setTokenProvider,
  ApiError,
  type ApiClientConfig,
  type RetryConfig,
} from './customFetch'

// Client and endpoint classes
export {
  VerticalSliceClient,
  getVerticalSliceClient,
  resetVerticalSliceClient,
  AuditEndpoint,
} from './client'

// React hooks
export { useVerticalSliceClient, useVerticalSliceAuth } from './useVerticalSliceClient'

// Re-export all generated types
export type * from './generated/model'

// Re-export generated functions for direct use if needed
export * from './generated/client'
