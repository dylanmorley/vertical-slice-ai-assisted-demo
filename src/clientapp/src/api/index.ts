
// Configuration and error handling
export {
  configureApi,
  getBaseUrl,
  setTokenProvider,
  ApiError,
  type ApiClientConfig,
  type RetryConfig,
} from './customFetch'

// React hooks
export { useVerticalSliceClient, useVerticalSliceAuth } from './useVerticalSliceClient'

// Generated API types
export type * from './generated/model'

// Generated API functions - use these directly for API calls
export * from './generated/client'
