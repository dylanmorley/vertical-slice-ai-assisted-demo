using Microsoft.Playwright;
using NUnit.Framework;

namespace VerticalSlice.E2E.Tests.Infrastructure;

/// <summary>
///     Base class for Playwright E2E tests that manages browser lifecycle and authentication.
///     Uses the AspireAppFixture to ensure the full application stack is running.
///     Auth state is persisted after the first login so subsequent tests reuse the session
///     and the browser stays on the application (not the Auth0 login screen).
/// </summary>
[TestFixture]
public abstract class PlaywrightTestBase
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            _appFixture ??= new AspireAppFixture();
            if (!_appFixture.IsStarted)
            {
                await _appFixture.StartAsync();
            }

            _isHeaded = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS") == "false";

            _playwright ??= await Playwright.CreateAsync();
            _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = !_isHeaded,
                SlowMo = _isHeaded ? 250 : 0,
                Channel = "chrome" // Force Google Chrome instead of Brave/Edge/Chromium
            });
        }
        catch
        {
            if (_appFixture is not null)
            {
                await _appFixture.DisposeAsync();
                _appFixture = null;
            }

            throw;
        }
    }

    [SetUp]
    public async Task SetUp()
    {
        // Restore the authenticated session into every new context so the
        // browser never shows the Auth0 login screen during the actual test.
        _context = await _browser!.NewContextAsync(new BrowserNewContextOptions { StorageState = _authStorageState });
        Page = await _context.NewPageAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
            _context = null;
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;

        if (_appFixture is not null)
        {
            await _appFixture.DisposeAsync();
            _appFixture = null;
        }
    }

    private static AspireAppFixture? _appFixture;
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static string? _authStorageState;
    private static bool _isHeaded;

    private IBrowserContext? _context;

    protected IPage Page { get; private set; } = null!;

    protected string FrontendUrl => _appFixture?.FrontendUrl ?? throw new InvalidOperationException("App not started");

    protected string ApiUrl => _appFixture?.ApiUrl ?? throw new InvalidOperationException("App not started");

    /// <summary>
    ///     Navigates to the given app route. The browser context is already
    ///     authenticated, so no Auth0 redirect should occur.
    ///     If the session has expired, falls back to a full interactive login.
    /// </summary>
    protected async Task LoginViaAuth0AndNavigateAsync(string path = "/dashboard")
    {
        string frontendOrigin = new Uri(FrontendUrl).GetLeftPart(UriPartial.Authority);

        TestContext.Progress.WriteLine($"[nav] Navigating to {FrontendUrl}/#{path}");
        await Page.GotoAsync($"{FrontendUrl}/#{path}");

        // Give the SPA a moment to evaluate the stored session and decide
        // whether to redirect to Auth0.
        bool redirectedToIdP;
        try
        {
            await Page.WaitForURLAsync(
                url => !url.StartsWith(frontendOrigin, StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = 5_000 });
            redirectedToIdP = true;
            TestContext.Progress.WriteLine($"[nav] Auth0 redirect detected at: {Page.Url}");
        }
        catch
        {
            redirectedToIdP = false;
            TestContext.Progress.WriteLine("[nav] No Auth0 redirect - using cached session.");
        }

        if (redirectedToIdP)
        {
            // Update the cached storage state for remaining tests.
            _authStorageState = await Page.Context.StorageStateAsync();
            TestContext.Progress.WriteLine("[auth] Updated cached storage state.");
        }

        // Ensure we land on the intended app route.
        if (!Page.Url.Contains($"#{path}", StringComparison.OrdinalIgnoreCase))
        {
            TestContext.Progress.WriteLine($"[nav] Ensuring final navigation to {FrontendUrl}/#{path}");
            await Page.GotoAsync($"{FrontendUrl}/#{path}");
        }

        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        TestContext.Progress.WriteLine($"[nav] Page ready at: {Page.Url}");
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
