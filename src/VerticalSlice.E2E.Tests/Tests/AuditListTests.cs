using Microsoft.Playwright;
using NUnit.Framework;
using VerticalSlice.E2E.Tests.Infrastructure;

namespace VerticalSlice.E2E.Tests.Tests;

[TestFixture]
public class AuditListTests : PlaywrightTestBase
{
    [Test]
    public async Task Should_Display_Audit_List_From_Dashboard()
    {
        await LoginViaAuth0AndNavigateAsync();

        await Expect(Page.Locator("text=Dashboard").First)
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 10_000 });

        Task<IResponse> nodesResponseTask = Page.WaitForResponseAsync(
            r => r.Request.Method == "GET" && r.Url.Contains("/api/v1/nodes", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = 60_000 });

        await Page.GotoAsync($"{FrontendUrl}/#/nodes");
        await Page.WaitForURLAsync(url => url.Contains("#/nodes"), new PageWaitForURLOptions { Timeout = 30_000 });

        IResponse? nodesResponse = null;
        try
        {
            nodesResponse = await nodesResponseTask;
        }
        catch
        {
            // If the nodes call didn't happen, the UI will show an error or stay in loading state.
        }

        if (nodesResponse is not null)
        {
            nodesResponse.Headers.TryGetValue("www-authenticate", out string? wwwAuthenticate);
            if (!string.IsNullOrWhiteSpace(wwwAuthenticate))
            {
                await TestContext.Progress.WriteLineAsync($"[nodes api] www-authenticate: {wwwAuthenticate}");
            }
        }

        ILocator heading = Page.Locator("h4:has-text('Risk Nodes')");
        await Expect(heading).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 30_000 });

        ILocator errorAlert = Page.Locator(".alert.alert-danger");
        if (await errorAlert.CountAsync() > 0)
        {
            string errorText = await errorAlert.First.InnerTextAsync();
            Assert.Fail($"Nodes list error: {errorText}");
        }

        ILocator nodeRows = Page.Locator("tbody tr");
        await Expect(nodeRows.First).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 60_000 });
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
