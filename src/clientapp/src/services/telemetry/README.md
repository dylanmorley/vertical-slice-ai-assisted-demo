# OpenTelemetry Configuration

## Environment Variables

Add these environment variables to your `.env` file or set them in your deployment environment:

```bash
# Enable/disable telemetry (default: true)
VITE_TELEMETRY_ENABLED=true

# OTLP endpoint for trace export (default: http://localhost:4318/v1/traces)
VITE_OTLP_ENDPOINT=http://localhost:4318/v1/traces

# Enable debug logging (default: false)
VITE_TELEMETRY_DEBUG=false
```

## Configuration Options

The TelemetryProvider accepts the following configuration options:

```typescript
interface TelemetryConfig {
  serviceName?: string // Default: 'vertical-slice-client'
  serviceVersion?: string // Default: '1.0.0'
  otlpEndpoint?: string // Default: from VITE_OTLP_ENDPOINT
  enabled?: boolean // Default: from VITE_TELEMETRY_ENABLED
  debug?: boolean // Default: from VITE_TELEMETRY_DEBUG
}
```

## Usage Examples

### Basic Usage (with defaults)

```tsx
import { TelemetryProvider } from './services/telemetry/TelemetryProvider'

function App() {
  return <TelemetryProvider>{/* Your app components */}</TelemetryProvider>
}
```

### Custom Configuration

```tsx
import { TelemetryProvider } from './services/telemetry/TelemetryProvider'

function App() {
  return (
    <TelemetryProvider
      config={{
        serviceName: 'my-vertical-slice-client',
        serviceVersion: '2.0.0',
        otlpEndpoint: 'https://my-otel-collector.com/v1/traces',
        debug: true,
      }}
    >
      {/* Your app components */}
    </TelemetryProvider>
  )
}
```

## What Gets Traced

The OpenTelemetry integration automatically traces:

1. **HTTP Requests**: All API calls made through the HttpClient
2. **Document Load**: Page load performance
3. **User Interactions**: Clicks and form submissions
4. **Custom Spans**: Manual instrumentation using the provided utilities

## Trace Attributes

Each HTTP request span includes:

- `http.method`: HTTP method (GET, POST, etc.)
- `http.url`: Full request URL
- `http.status_code`: Response status code
- `http.status_text`: Response status text
- `service.name`: Service identifier
- `api.endpoint`: API endpoint path
- `api.resource`: API resource name

## Manual Instrumentation

You can create custom spans for specific operations:

```typescript
import { createSpan, withTracing } from './services/telemetry'

// Simple span
const span = createSpan('my-operation', { 'custom.attr': 'value' })
// ... do work ...
span.end()

// Wrapped function
const tracedFunction = withTracing(
  async (data: any) => {
    // Your async operation
    return processData(data)
  },
  'process-data',
  { 'operation.type': 'data-processing' },
)
```
