using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace VerticalSlice.Web.Api.Telemetry;

[ExcludeFromCodeCoverage]
public static class OpenTelemetryConfiguration
{
    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        string serviceName = "VerticalSlice.API";
        string serviceVersion = "1.0.0";

        // Get OTLP configuration from appsettings
        string otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                              ?? configuration["OpenTelemetry:OtlpEndpoint"]
                              ?? "http://localhost:4317";

        string otlpHeaders = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS")
                             ?? configuration["OpenTelemetry:OtlpHeaders"]
                             ?? string.Empty;

        // Create resource with service information
        ResourceBuilder resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] =
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ["service.instance.id"] = Environment.MachineName
            });

        // Configure tracing
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Configure ASP.NET Core instrumentation
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.body.size", httpRequest.ContentLength);
                            activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                            activity.SetTag("http.response.body.size", httpResponse.ContentLength);
                        options.EnrichWithException = (activity, exception) =>
                        {
                            activity.SetTag("exception.type", exception.GetType().Name);
                            activity.SetTag("exception.message", exception.Message);
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.client.request.method", request.Method.Method);
                            activity.SetTag("http.client.request.url", request.RequestUri?.ToString());
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddSqlClientInstrumentation(options => { options.RecordException = true; })
                    .AddSource("VerticalSlice.Audit")
                    .AddSource("VerticalSlice.Commands")
                    .AddSource("VerticalSlice.Queries")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        if (!string.IsNullOrEmpty(otlpHeaders))
                        {
                            options.Headers = otlpHeaders;
                        }

                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("VerticalSlice.API")
                    .AddMeter("VerticalSlice.Audit")
                    .AddMeter("VerticalSlice.Commands")
                    .AddMeter("VerticalSlice.Queries")
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        if (!string.IsNullOrEmpty(otlpHeaders))
                        {
                            options.Headers = otlpHeaders;
                        }

                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        // Configure logging
        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource)
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otlpEndpoint);
                        if (!string.IsNullOrEmpty(otlpHeaders))
                        {
                            otlpOptions.Headers = otlpHeaders;
                        }

                        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                    });
            });
        });
    }
}
