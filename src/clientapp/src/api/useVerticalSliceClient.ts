/**
 * React Hook for Vertical Slice API Client with automatic authentication
 *
 * This hook integrates the API client with the UserContext to provide
 * automatic token management and authentication.
 *
 * @example
 * import { useVerticalSliceClient } from '../api';
 *
 * function MyComponent() {
 *   const apiClient = useVerticalSliceClient();
 *
 *   useEffect(() => {
 *     async function loadData() {
 *       const nodes = await apiClient.nodes.getAll();
 *       console.log(nodes);
 *     }
 *     loadData();
 *   }, [apiClient]);
 * }
 */

import { useEffect, useRef } from 'react'
import { useUser } from '../contexts/UserContext'
import { configureApi, setTokenProvider } from './customFetch'
import { getVerticalSliceClient, VerticalSliceClient } from './client'

// Track if we've configured the API
let isConfigured = false

/**
 * Hook that provides a VerticalSliceClient instance configured with the current user's token
 *
 * @returns VerticalSliceClient instance with automatic authentication
 */
export function useVerticalSliceClient(): VerticalSliceClient {
  const { getToken } = useUser()
  const clientRef = useRef<VerticalSliceClient | null>(null)

  // Configure API on first use
  if (!isConfigured) {
    configureApi({
      tokenProvider: getToken,
    })
    isConfigured = true
  }

  // Initialize the client on first render
  if (!clientRef.current) {
    clientRef.current = getVerticalSliceClient()
  }

  // Update token provider if getToken changes
  useEffect(() => {
    setTokenProvider(getToken)
  }, [getToken])

  return clientRef.current
}

/**
 * Hook for components that need direct access to getToken from UserContext
 * This is useful when you want to initialize the client yourself
 *
 * @example
 * const { getToken } = useVerticalSliceAuth();
 */
export function useVerticalSliceAuth() {
  const { getToken, isAuthenticated, isLoading } = useUser()

  return {
    getToken,
    isAuthenticated,
    isLoading,
  }
}
