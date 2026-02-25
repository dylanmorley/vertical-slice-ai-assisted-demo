using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace VerticalSlice.Web.Api.Data;

/// <summary>
///     Seeds the database using configurable seed data providers.
///     Supports multiple data sets (Intelligence, Ecommerce, Financial Institution).
///     Default is Intelligence Community data set.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DatabaseSeeder
{

    /// <summary>
    ///     Seeds the database using the provided seed data provider.
    /// </summary>
    public static async Task SeedAsync(
        VerticalSliceDataContext context)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed data in order of dependencies
        await SeedAuditsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAuditsAsync(VerticalSliceDataContext context)
    {
        if (await context.Audit.AnyAsync())
        {
            return;
        }

        var rand = new Random();
        var now = DateTime.UtcNow;

        string[] operations = { "CREATE", "UPDATE", "DELETE", "READ", "LOGIN", "LOGOUT", "IMPORT", "EXPORT", "PATCH" };
        string[] entityTypes = { "Node", "Organization", "User", "RiskLink", "NodeType", "Geography", "Alert", "Report" };
        (string id, string name)[] users =
        {
            ("u-1", "alice@example.com"),
            ("u-2", "bob@example.com"),
            ("u-3", "carol@example.com"),
            ("u-4", "dave@example.com"),
            ("u-5", "eve@example.com")
        };
        string[] methods = { "GET", "POST", "PUT", "PATCH", "DELETE" };
        string[] endpoints = { "/api/v1/nodes", "/api/v1/organizations", "/api/v1/audit", "/api/v1/node-types", "/api/v1/risk-links" };
        string[] userAgents = { "Mozilla/5.0", "curl/7.79.1", "PostmanRuntime/7.29.0", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)" };

        var audits = new List<Model.Audit>(capacity: 500);

        for (int i = 0; i < 500; i++)
        {
            string op = operations[rand.Next(operations.Length)];
            string entity = entityTypes[rand.Next(entityTypes.Length)];
            string entityId = (rand.Next(1, 2000)).ToString();
            var user = users[rand.Next(users.Length)];

            DateTime timestamp = now - TimeSpan.FromDays(rand.Next(0, 90)) - TimeSpan.FromSeconds(rand.Next(0, 86400));

            string? oldValues = null;
            string? newValues = null;
            if (op == "UPDATE")
            {
                oldValues = $"{{ \"name\": \"{entity}-old-{rand.Next(1,1000)}\" }}";
                newValues = $"{{ \"name\": \"{entity}-new-{rand.Next(1,1000)}\" }}";
            }
            else if (op == "CREATE")
            {
                newValues = $"{{ \"name\": \"{entity}-created-{rand.Next(1,10000)}\" }}";
            }

            var audit = new Model.Audit
            {
                Operation = op,
                EntityType = entity,
                EntityId = entityId,
                UserId = user.id,
                UserName = user.name,
                Timestamp = timestamp,
                IpAddress = $"{rand.Next(1,255)}.{rand.Next(0,255)}.{rand.Next(0,255)}.{rand.Next(0,255)}",
                UserAgent = userAgents[rand.Next(userAgents.Length)],
                OldValues = oldValues,
                NewValues = newValues,
                Context = rand.NextDouble() > 0.9 ? "batch-import" : null,
                HttpMethod = methods[rand.Next(methods.Length)],
                Endpoint = endpoints[rand.Next(endpoints.Length)],
                IsSuccess = rand.NextDouble() > 0.05,
                ErrorMessage = null,
                OrganizationId = rand.NextDouble() > 0.7 ? $"org-{rand.Next(1,20)}" : null,
                Tags = rand.NextDouble() > 0.85 ? "automated,import" : null,
                DurationMs = rand.Next(5, 5000),
                CorrelationId = Guid.NewGuid().ToString()
            };

            if (!audit.IsSuccess)
            {
                audit.ErrorMessage = "Simulated failure: unexpected error";
            }

            audits.Add(audit);
        }

        await context.Audit.AddRangeAsync(audits);
    }
}
