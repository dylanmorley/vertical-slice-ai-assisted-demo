/**
 * VerticalSliceClient - Wrapper providing backward-compatible interface over generated API client
 *
 * This provides the `apiClient.nodes.getAll()` style interface that the application uses,
 * while delegating to the Orval-generated functions under the hood.
 */

import {
  // Audit
  getAuditRecords,
} from './generated/client'

import type {
  GetAuditRecordsParams,
  PagedAuditResponse,
} from './generated/model'

// Re-export types for consumers
export type * from './generated/model'

/**
 * Helper to extract data from response.
 * The customFetch throws on errors, so by the time we reach here,
 * we know the response is a success response with data.
 *
 * customFetch returns: { data: <API response body>, status, headers }
 * API response body varies:
 *   - Simple list: { data: T[] } -> we want T[]
 *   - Paginated: { data: { data: T[], pagination: {...} } } -> we want { data: T[], pagination: {...} }
 *   - Single item: { data: T } -> we want T
 */
function extractData<T>(response: unknown): T {
  const outer = (response as { data: unknown }).data
  // Check if the API response has a nested 'data' property but NOT a pagination property
  // (paginated responses have both 'data' and 'pagination' and should be returned as-is)
  if (outer && typeof outer === 'object' && 'data' in outer && !('pagination' in outer)) {
    return (outer as { data: T }).data
  }
  // Return as-is for paginated responses or simple responses
  return outer as T
}


export class AuditEndpoint {
  async getRecords(params?: GetAuditRecordsParams): Promise<PagedAuditResponse> {
    const response = await getAuditRecords(params)
    return extractData(response)
  }
}

import { customFetch } from './customFetch'

export class VerticalSliceClient {
  public readonly audit = new AuditEndpoint()
}

// Create singleton instance
let clientInstance: VerticalSliceClient | null = null

/**
 * Get or create the singleton VerticalSliceClient instance
 */
export function getVerticalSliceClient(): VerticalSliceClient {
  if (!clientInstance) {
    clientInstance = new VerticalSliceClient()
  }
  return clientInstance
}

/**
 * Reset the singleton instance (useful for testing)
 */
export function resetVerticalSliceClient(): void {
  clientInstance = null
}
