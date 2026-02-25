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

        Task<IResponse> auditResponseTask = Page.WaitForResponseAsync(
            r => r.Request.Method == "GET" && r.Url.Contains("/api/v1/audit", StringComparison.OrdinalIgnoreCase),
            new PageWaitForResponseOptions { Timeout = 60_000 });

        await Page.GotoAsync($"{FrontendUrl}/#/administration/audit");
        await Page.WaitForURLAsync(url => url.Contains("#/administration/audit"), new PageWaitForURLOptions { Timeout = 30_000 });

        await auditResponseTask;

        ILocator auditRows = Page.Locator("tbody tr");
        await Expect(auditRows.First).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 30_000 });
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
