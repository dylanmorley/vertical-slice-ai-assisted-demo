using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Projects;

namespace VerticalSlice.E2E.Tests.Infrastructure;

/// <summary>
///     Manages the Aspire distributed application lifecycle for E2E tests.
///     Starts the full application stack (API + Frontend) using Aspire orchestration.
/// </summary>
public class AspireAppFixture : IAsyncDisposable
{
    private DistributedApplication? _app;

    /// <summary>
    ///     Gets the frontend URL once the app is started.
    /// </summary>
    public string FrontendUrl { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the API URL once the app is started.
    /// </summary>
    public string ApiUrl { get; private set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the application has been started.
    /// </summary>
    public bool IsStarted => _app is not null;

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
        }
    }

    /// <summary>
    ///     Starts the distributed application for E2E tests.
    /// </summary>
    public async Task StartAsync()
    {
        if (_app is not null)
        {
            return;
        }

        // Use Development so the API uses real Auth0 JWT validation.
        // E2E authentication happens via the Auth0 login screen.
        const string environmentName = "Development";
        IDistributedApplicationTestingBuilder appHost =
            await DistributedApplicationTestingBuilder.CreateAsync<VerticalSlice_AppHost>(
                ["--environment", environmentName],
                (_, settings) => { settings.EnvironmentName = environmentName; });

        _app = await appHost.BuildAsync();

        await _app.StartAsync();

        // Wait for the API to be ready using the resource notification service
        ResourceNotificationService resourceNotificationService =
            _app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceHealthyAsync("verticalsliceapi");

        // Give the frontend a moment to start (it doesn't have health checks)
        await Task.Delay(500);

        ApiUrl = GetResourceUrl("verticalsliceapi");

        // Auth0 requires a stable origin; the UI must be reachable at this exact URL.
        // Aspire may still report a dynamically allocated endpoint for the JavaScript resource,
        // so we explicitly use the required localhost:5173 origin.
        string aspireReportedFrontendUrl = GetResourceUrl("verticalsliceui");
        TestContext.Progress.WriteLine($"[ui] Aspire reported: {aspireReportedFrontendUrl}");

        FrontendUrl = "http://localhost:5173";
        await WaitForFrontendReadyAsync(FrontendUrl);

        await VerifyApiHealthyAsync();
    }

    private static async Task WaitForFrontendReadyAsync(string frontendUrl)
    {
        using HttpClient httpClient = new();
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(60));

        while (!cts.IsCancellationRequested)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"{frontendUrl}/", cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    string html = await response.Content.ReadAsStringAsync(cts.Token);
                    if (html.Contains("<title>Vertical Slice</title>", StringComparison.OrdinalIgnoreCase))
                    {
                        TestContext.Progress.WriteLine($"[ui] GET / -> {(int)response.StatusCode}");
                        return;
                    }
                }
            }
            catch
            {
                // ignore and retry
            }

            await Task.Delay(500, cts.Token);
        }

        throw new InvalidOperationException($"UI did not become ready at {frontendUrl} within timeout.");
    }

    private async Task VerifyApiHealthyAsync()
    {
        try
        {
            using HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync($"{ApiUrl}/health");
            TestContext.Progress.WriteLine($"[api smoke] GET /health -> {(int)response.StatusCode}");
            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();
                TestContext.Progress.WriteLine($"[api smoke] body: {body}");
            }
        }
        catch (Exception ex)
        {
            TestContext.Progress.WriteLine($"[api smoke] failed: {ex}");
        }
    }

    private string GetResourceUrl(string resourceName)
    {
        Uri endpoint = _app!.GetEndpoint(resourceName, "http");
        return endpoint.ToString().TrimEnd('/');
    }
}
