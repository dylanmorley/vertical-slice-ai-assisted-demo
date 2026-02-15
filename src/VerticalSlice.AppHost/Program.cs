using VerticalSlice.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var verticalSliceApi = builder.AddProject<Projects.VerticalSlice_Web_Api>("verticalsliceapi")
    .WithEnvironment("VERTICALSLICE_SEED_DATA_SET", "FinancialInstitution")
    .WithEnvironment("ExternalFeedApi__Enabled", "true")
    .WithOtlpExporter()
    .PublishAsAzureContainerApp((infra, app) => app.Configuration.Ingress.AllowInsecure = true);

builder.AddViteApp("verticalsliceui", "../clientapp")
    .WithNpm()
    .WithEnvironment("PORT", "5173")
    .WithEnvironment("NODE_ENV", "development")
    .WithEnvironment("VITE_API_URL", "/api/v1")
    .WithEnvironment("VITE_TELEMETRY_ENABLED", "false")
    .WithEnvironment("VERTICALSLICE_E2E", "true")
    .WithReverseProxy(verticalSliceApi.GetEndpoint("http"))
    .WithEndpoint("http", (endpointAnnotation) =>
    {
        endpointAnnotation.IsProxied = false;
        endpointAnnotation.Port = 5173;
    });

builder.Build().Run();
