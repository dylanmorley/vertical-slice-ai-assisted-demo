using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace VerticalSlice.Web.Api.Telemetry;

public static class QueryTelemetry
{
    private static readonly ActivitySource QueryActivitySource = new("VerticalSlice.Queries");
    private static readonly Meter QueryMeter = new("VerticalSlice.Queries");

    // Metrics
    private static readonly Counter<long> QueryExecutionsCounter =
        QueryMeter.CreateCounter<long>("query_executions_total", "Total number of query executions");

    private static readonly Histogram<double> QueryExecutionDuration =
        QueryMeter.CreateHistogram<double>("query_execution_duration_seconds",
            "Duration of query executions in seconds");

    private static readonly Counter<long> QueryFailuresCounter =
        QueryMeter.CreateCounter<long>("query_failures_total", "Total number of query failures");

    public static Activity? StartQueryExecution<TQuery>(TQuery query)
        where TQuery : class
    {
        string queryName = typeof(TQuery).Name;
        Activity? activity = QueryActivitySource.StartActivity($"query.{queryName.ToLower()}");

        if (activity != null)
        {
            activity.SetTag("query.name", queryName);
            activity.SetTag("query.type", typeof(TQuery).FullName ?? queryName);
            activity.SetTag("query.timestamp", DateTime.UtcNow.ToString("O"));
        }

        return activity;
    }

    public static void RecordQueryExecution<TQuery>(TQuery query, bool isSuccess,
        double durationSeconds, string? errorMessage = null)
        where TQuery : class
    {
        string queryName = typeof(TQuery).Name;

        // Record metrics
        TagList tags = new() { { "query_name", queryName }, { "success", isSuccess.ToString() } };

        QueryExecutionsCounter.Add(1, tags);
        QueryExecutionDuration.Record(durationSeconds, tags);

        if (!isSuccess)
        {
            TagList failureTags = new() { { "query_name", queryName }, { "error", errorMessage ?? "unknown" } };
            QueryFailuresCounter.Add(1, failureTags);
        }
    }

    public static void EnrichQueryActivity<TQuery>(Activity activity, TQuery query)
        where TQuery : class
    {
        if (activity == null)
        {
            return;
        }

        activity.SetTag("query.timestamp", DateTime.UtcNow.ToString("O"));

        // Add query-specific properties using reflection
        PropertyInfo[] properties = typeof(TQuery).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.Name.EndsWith("Id") && property.CanRead)
            {
                object? value = property.GetValue(query);
                if (value != null)
                {
                    activity.SetTag($"query.{property.Name.ToLower()}", value.ToString());
                }
            }
        }
    }
}
