using System.Diagnostics;
using System.Diagnostics.Metrics;
using VerticalSlice.Web.Api.Model;

namespace VerticalSlice.Web.Api.Telemetry;

public static class AuditTelemetry
{
    private static readonly ActivitySource AuditActivitySource = new("VerticalSlice.Audit");
    private static readonly Meter AuditMeter = new("VerticalSlice.Audit");

    // Metrics
    private static readonly Counter<long> AuditOperationsCounter =
        AuditMeter.CreateCounter<long>("audit_operations_total", "Total number of audit operations");

    private static readonly Histogram<double> AuditOperationDuration =
        AuditMeter.CreateHistogram<double>("audit_operation_duration_seconds",
            "Duration of audit operations in seconds");

    private static readonly Counter<long> AuditFailuresCounter =
        AuditMeter.CreateCounter<long>("audit_failures_total", "Total number of audit failures");

    public static Activity? StartAuditOperation(string operation, string entityType, string entityId)
    {
        Activity? activity = AuditActivitySource.StartActivity($"audit.{operation.ToLower()}");

        if (activity != null)
        {
            activity.SetTag("audit.operation", operation);
            activity.SetTag("audit.entity_type", entityType);
            activity.SetTag("audit.entity_id", entityId);
            activity.SetTag("audit.timestamp", DateTime.UtcNow.ToString("O"));
        }

        return activity;
    }

    public static void RecordAuditOperation(string operation, string entityType, string entityId,
        bool isSuccess, double durationSeconds, string? errorMessage = null)
    {
        // Record metrics
        TagList tags = new()
        {
            { "operation", operation }, { "entity_type", entityType }, { "success", isSuccess.ToString() }
        };

        AuditOperationsCounter.Add(1, tags);
        AuditOperationDuration.Record(durationSeconds, tags);

        if (!isSuccess)
        {
            TagList failureTags = new()
            {
                { "operation", operation }, { "entity_type", entityType }, { "error", errorMessage ?? "unknown" }
            };
            AuditFailuresCounter.Add(1, failureTags);
        }
    }

    public static void EnrichAuditActivity(Activity activity, Audit audit)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetTag("audit.user_id", audit.UserId);
        activity.SetTag("audit.user_name", audit.UserName);
        activity.SetTag("audit.organization_id", audit.OrganizationId);
        activity.SetTag("audit.ip_address", audit.IpAddress);
        activity.SetTag("audit.user_agent", audit.UserAgent);
        activity.SetTag("audit.http_method", audit.HttpMethod);
        activity.SetTag("audit.endpoint", audit.Endpoint);
        activity.SetTag("audit.correlation_id", audit.CorrelationId);
        activity.SetTag("audit.duration_ms", audit.DurationMs);

        if (!string.IsNullOrEmpty(audit.ErrorMessage))
        {
            activity.SetTag("audit.error_message", audit.ErrorMessage);
            activity.SetStatus(ActivityStatusCode.Error, audit.ErrorMessage);
        }
        else
        {
            activity.SetStatus(ActivityStatusCode.Ok);
        }
    }
}
