using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using VerticalSlice.Web.Api.Data.SeedDataProviders;
using VerticalSlice.Web.Api.Model;

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
    ///     Environment variable name for specifying the seed data set.
    /// </summary>
    public const string SeedDataSetEnvVar = "VERTICALSLICE_SEED_DATA_SET";

    /// <summary>
    ///     Seeds the database with the default data set (Intelligence Community).
    /// </summary>
    public static Task SeedAsync(VerticalSliceDataContext context, string? environment = null) =>
        SeedAsync(context, SeedDataSet.FinancialInstitution);

    /// <summary>
    ///     Seeds the database with the specified data set.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="dataSet">The seed data set to use.</param>
    public static async Task SeedAsync(VerticalSliceDataContext context, SeedDataSet dataSet)
    {
        ISeedDataProvider provider = SeedDataProviderFactory.Create(dataSet);
        await SeedWithProviderAsync(context, provider);
    }

    /// <summary>
    ///     Seeds the database using the data set specified in the environment variable NODEGUARD_SEED_DATA_SET.
    ///     Falls back to the specified default if the environment variable is not set.
    /// </summary>
    public static Task SeedFromEnvironmentAsync(
        VerticalSliceDataContext context,
        SeedDataSet defaultDataSet = SeedDataSet.FinancialInstitution)
    {
        string? envValue = Environment.GetEnvironmentVariable(SeedDataSetEnvVar);
        SeedDataSet dataSet = SeedDataProviderFactory.ParseDataSet(envValue, defaultDataSet);
        return SeedAsync(context, dataSet);
    }

    /// <summary>
    ///     Seeds the database using the provided seed data provider.
    /// </summary>
    public static async Task SeedWithProviderAsync(
        VerticalSliceDataContext context,
        ISeedDataProvider provider)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed data in order of dependencies
        await SeedGeographiesAsync(context, provider);
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

    private static async Task SeedGeographiesAsync(VerticalSliceDataContext context, ISeedDataProvider provider)
    {
        if (await context.Geography.AnyAsync())
        {
            return;
        }

        List<Geography> geographies = provider.GetGeographies();
        await context.Geography.AddRangeAsync(geographies);
    }
}
