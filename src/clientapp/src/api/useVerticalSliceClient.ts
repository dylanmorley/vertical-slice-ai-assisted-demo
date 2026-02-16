/**
 * React Hook for Vertical Slice API Client with automatic authentication
 *
 * This hook configures the API client with the current user's authentication token.
 * After calling this hook once in your app/component tree, all Orval-generated
 * API functions will automatically include the auth token in requests.
 *
 * @example
 * import { useVerticalSliceClient, getAuditRecords } from '../api';
 *
 * function MyComponent() {
 *   useVerticalSliceClient(); // Configure auth
 *
 *   useEffect(() => {
 *     async function loadData() {
 *       const response = await getAuditRecords({ page: 1, pageSize: 50 });
 *       console.log(response.data);
 *     }
 *     loadData();
 *   }, []);
 * }
 */

import { useEffect } from 'react'
import { useUser } from '../contexts/UserContext'
import { configureApi, setTokenProvider } from './customFetch'

// Track if we've configured the API
let isConfigured = false

/**
 * Hook that configures the API client with the current user's authentication token.
 * Call this once in your component or app root to enable authenticated API requests.
 * All Orval-generated API functions will then automatically include the auth token.
 */
export function useVerticalSliceClient(): void {
  const { getToken } = useUser()

  // Configure API on first use
  if (!isConfigured) {
    configureApi({
      tokenProvider: getToken,
    })
    isConfigured = true
  }

  // Update token provider if getToken changes
  useEffect(() => {
    setTokenProvider(getToken)
  }, [getToken])
}
