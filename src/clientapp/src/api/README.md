# API Client Usage

This directory contains the Orval-generated TypeScript client for the Vertical Slice API, along with custom authentication and error handling.

## Quick Start

### 1. Configure Authentication

Call `useVerticalSliceClient()` once in your component or app root to configure authentication:

```tsx
import { useVerticalSliceClient } from './api'

function MyComponent() {
  useVerticalSliceClient() // Configure auth - that's it!
  
  // ... rest of your component
}
```

### 2. Use Generated API Functions Directly

Import and use the generated API functions directly - no wrapper needed:

```tsx
import { useVerticalSliceClient, getAuditRecords } from './api'
import type { GetAuditRecordsParams } from './api'

function AuditList() {
  useVerticalSliceClient() // Configure auth
  const [audits, setAudits] = useState([])

  useEffect(() => {
    async function loadData() {
      const params: GetAuditRecordsParams = {
        page: 1,
        pageSize: 50
      }
      
      const response = await getAuditRecords(params)
      setAudits(response.data.data) // response.data contains the API response
    }
    loadData()
  }, [])

  // ... render audits
}
```

## How It Works

### Generated Code

The `generated/` folder contains:
- `client.ts` - Orval-generated API functions (e.g., `getAuditRecords`, `createUser`, etc.)
- `model/` - TypeScript types for all API request/response models

These files are regenerated whenever you run `npm run generate-client`.

### Custom Fetch

All generated functions use `customFetch.ts` as their mutator, which provides:
- ✅ Automatic authentication (Bearer token injection)
- ✅ Retry logic with exponential backoff
- ✅ ProblemDetails error parsing
- ✅ OpenTelemetry tracing
- ✅ Timeout handling

### Authentication Flow

1. `useVerticalSliceClient()` gets the `getToken` function from `UserContext`
2. It configures `customFetch` to use that token provider
3. Every API call automatically includes `Authorization: Bearer <token>` when a token is available
4. Token updates are tracked via React's `useEffect`

## Response Format

All Orval-generated functions return a response in this format:

```typescript
{
  data: T,           // The actual response data from the API
  status: number,    // HTTP status code
  headers: Headers   // Response headers
}
```

So if your API returns `PagedAuditResponse`, access it via `response.data`.

## Error Handling

The custom fetch throws `ApiError` instances with rich error information:

```tsx
import { ApiError, getAuditRecords } from './api'

try {
  const response = await getAuditRecords(params)
  // Use response.data
} catch (error) {
  if (error instanceof ApiError) {
    console.error('Status:', error.statusCode)
    console.error('Message:', error.getUserFriendlyMessage())
    console.error('All errors:', error.getAllErrorMessages())
    
    if (error.isUnauthorized()) {
      // Handle 401
    } else if (error.isBadRequest()) {
      // Handle 400
    }
  }
}
```

## Configuration

You can customize the API client configuration before using it:

```tsx
import { configureApi } from './api'

configureApi({
  baseUrl: 'https://api.example.com/api/v1',  // Override default
  timeout: 60000,                              // 60 second timeout
  retry: {
    maxRetries: 5,
    retryDelay: 2000
  },
  onUnauthorized: (error) => {
    // Custom 401 handler
    window.location.href = '/login'
  },
  onError: (error) => {
    // Global error handler
    console.error('API Error:', error)
  }
})
```

## Regenerating the Client

When the backend API changes:

1. Build the backend to generate `openapi.json`:
   ```bash
   cd ../VerticalSlice.Web.Api
   dotnet build
   ```

2. Regenerate the TypeScript client:
   ```bash
   cd ../../clientapp
   npm run generate-client
   ```

The Orval configuration is in `orval.config.ts`.

## Migration from Old Pattern

**Old (with wrapper):**
```tsx
const apiClient = useVerticalSliceClient()
const response = await apiClient.audit.getRecords(params)
const data = response.data
```

**New (simplified):**
```tsx
useVerticalSliceClient() // Just configure auth
const response = await getAuditRecords(params)
const data = response.data.data // Note: response.data is the API response
```

## Files in This Directory

- `customFetch.ts` - Custom fetch implementation with auth, retry, error handling
- `useVerticalSliceClient.ts` - React hook to configure authentication
- `index.ts` - Main exports (import everything from here)
- `generated/` - Orval-generated code (do not edit manually)
  - `client.ts` - API functions
  - `model/` - TypeScript types
