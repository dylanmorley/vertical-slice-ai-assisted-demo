using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace VerticalSlice.Web.Api.Telemetry;

public static class CommandTelemetry
{
    private static readonly ActivitySource CommandActivitySource = new("VerticalSlice.Commands");
    private static readonly Meter CommandMeter = new("VerticalSlice.Commands");

    // Metrics
    private static readonly Counter<long> CommandExecutionsCounter =
        CommandMeter.CreateCounter<long>("command_executions_total", "Total number of command executions");

    private static readonly Histogram<double> CommandExecutionDuration =
        CommandMeter.CreateHistogram<double>("command_execution_duration_seconds",
            "Duration of command executions in seconds");

    private static readonly Counter<long> CommandFailuresCounter =
        CommandMeter.CreateCounter<long>("command_failures_total", "Total number of command failures");

    public static Activity? StartCommandExecution<TCommand>(TCommand command)
        where TCommand : class
    {
        string commandName = typeof(TCommand).Name;
        Activity? activity = CommandActivitySource.StartActivity($"command.{commandName.ToLower()}");

        if (activity != null)
        {
            activity.SetTag("command.name", commandName);
            activity.SetTag("command.type", typeof(TCommand).FullName ?? commandName);

            // Use reflection to get the Id property
            PropertyInfo? idProperty = typeof(TCommand).GetProperty("Id");
            if (idProperty != null)
            {
                object? id = idProperty.GetValue(command);
                if (id != null)
                {
                    activity.SetTag("command.id", id.ToString());
                }
            }

            activity.SetTag("command.timestamp", DateTime.UtcNow.ToString("O"));
        }

        return activity;
    }

    public static void RecordCommandExecution<TCommand>(TCommand command, bool isSuccess,
        double durationSeconds, string? errorMessage = null)
        where TCommand : class
    {
        string commandName = typeof(TCommand).Name;

        // Record metrics
        TagList tags = new() { { "command_name", commandName }, { "success", isSuccess.ToString() } };

        CommandExecutionsCounter.Add(1, tags);
        CommandExecutionDuration.Record(durationSeconds, tags);

        if (!isSuccess)
        {
            TagList failureTags = new() { { "command_name", commandName }, { "error", errorMessage ?? "unknown" } };
            CommandFailuresCounter.Add(1, failureTags);
        }
    }

    public static void EnrichCommandActivity<TCommand>(Activity activity, TCommand command)
        where TCommand : class
    {
        if (activity == null)
        {
            return;
        }

        // Use reflection to get the Id property
        PropertyInfo? idProperty = typeof(TCommand).GetProperty("Id");
        if (idProperty != null)
        {
            object? id = idProperty.GetValue(command);
            if (id != null)
            {
                activity.SetTag("command.id", id.ToString());
            }
        }

        activity.SetTag("command.timestamp", DateTime.UtcNow.ToString("O"));

        // Add command-specific properties using reflection
        PropertyInfo[] properties = typeof(TCommand).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.Name.EndsWith("Id") && property.CanRead)
            {
                object? value = property.GetValue(command);
                if (value != null)
                {
                    activity.SetTag($"command.{property.Name.ToLower()}", value.ToString());
                }
            }
        }
    }
}
