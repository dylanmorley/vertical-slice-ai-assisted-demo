import { WebTracerProvider } from '@opentelemetry/sdk-trace-web'
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch'
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load'
import { UserInteractionInstrumentation } from '@opentelemetry/instrumentation-user-interaction'
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http'
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base'
import { registerInstrumentations } from '@opentelemetry/instrumentation'
import { trace } from '@opentelemetry/api'

export interface TelemetryConfig {
  serviceName?: string
  serviceVersion?: string
  otlpEndpoint?: string
  enabled?: boolean
  debug?: boolean
}

/**
 * Initialize OpenTelemetry for the browser application
 */
export function initializeTelemetry(config: TelemetryConfig = {}): void {
  const {
    serviceName = 'vertical-slice-client',
    serviceVersion = '1.0.0',
    otlpEndpoint = import.meta.env.VITE_OTLP_ENDPOINT || 'http://localhost:4318/v1/traces',
    enabled = import.meta.env.VITE_TELEMETRY_ENABLED !== 'false',
    debug = import.meta.env.VITE_TELEMETRY_DEBUG === 'true',
  } = config

  if (!enabled) {
    console.log('OpenTelemetry tracing is disabled')
    return
  }

  try {
    const traceExporter = new OTLPTraceExporter({
      url: otlpEndpoint,
      headers: {},
    })

    const tracerProvider = new WebTracerProvider({
      spanProcessors: [new BatchSpanProcessor(traceExporter)],
    })

    tracerProvider.register()

    registerInstrumentations({
      instrumentations: [
        new FetchInstrumentation({
          // Only instrument requests to our API
          ignoreUrls: [
            /^https?:\/\/localhost:3000/, // Vite dev server
            /^https?:\/\/localhost:5173/, // Vite dev server alternative
            /^https?:\/\/fonts\.googleapis\.com/, // Google Fonts
            /^https?:\/\/fonts\.gstatic\.com/, // Google Fonts static
            /\.(css|js|png|jpg|jpeg|gif|svg|ico|woff|woff2|ttf|eot)$/i, // Static assets
          ],
          // Add custom attributes to spans
          applyCustomAttributesOnSpan: (span, request, result) => {
            span.setAttributes({
              'http.request.method': request.method,
              'http.response.status_code': result.status,
            })

            if (request.url.includes('/api/v1/')) {
              span.setAttributes({
                'service.name': 'vertical-slice-api',
                'api.version': 'v1',
              })

              // Extract endpoint information
              const url = new URL(request.url)
              const pathSegments = url.pathname.split('/').filter(Boolean)
              if (pathSegments.length >= 3) {
                span.setAttributes({
                  'api.endpoint': pathSegments.slice(2).join('/'),
                  'api.resource': pathSegments[2],
                })
              }
            }
          },
        }),
        new DocumentLoadInstrumentation(),
        new UserInteractionInstrumentation({
          eventNames: ['click', 'submit'],
          shouldPreventSpanCreation: (event, element) => {
            return element.matches(
              'nav a, .navbar a, .sidebar a, button[type="button"]:not([data-api-action])',
            )
          },
        }),
      ],
    })

    if (debug) {
      console.log('OpenTelemetry initialized successfully')
      console.log('Service:', serviceName, serviceVersion)
      console.log('OTLP Endpoint:', otlpEndpoint)
    }
  } catch (error) {
    console.error('Failed to initialize OpenTelemetry:', error)
  }
}

/**
 * Get the current tracer instance (browser-safe)
 */
export function getTracer(name: string = 'vertical-slice-client') {
  try {
    return trace.getTracer(name)
  } catch (error) {
    // Fallback: return a no-op tracer if OpenTelemetry is not available
    return {
      startSpan: (name: string, options?: any) => ({
        setAttributes: () => {},
        setStatus: () => {},
        recordException: () => {},
        end: () => {},
      }),
    }
  }
}
