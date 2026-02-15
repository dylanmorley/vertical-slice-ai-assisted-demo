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

            // Perform a single login and capture the storage state so every
            // subsequent test context starts already authenticated.
            if (_authStorageState is null)
            {
                TestContext.Progress.WriteLine("[auth] Acquiring authentication state via Auth0 login...");
                _authStorageState = await AcquireAuthStorageStateAsync();
                TestContext.Progress.WriteLine("[auth] Authentication state captured successfully.");
            }
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
            // Session expired – perform a fresh login.
            TestContext.Progress.WriteLine("[auth] Storage state expired, performing fresh login.");
            await PerformAuth0LoginAsync(frontendOrigin);

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

    /// <summary>
    ///     Opens a throwaway context, logs in through Auth0, and captures the
    ///     resulting cookies + localStorage as a JSON string that can be injected
    ///     into subsequent contexts via <see cref="BrowserNewContextOptions.StorageState" />.
    /// </summary>
    private async Task<string> AcquireAuthStorageStateAsync()
    {
        TestContext.Progress.WriteLine("[auth] Creating temporary context for initial login...");
        IBrowserContext ctx = await _browser!.NewContextAsync();
        IPage page = await ctx.NewPageAsync();
        string frontendOrigin = new Uri(FrontendUrl).GetLeftPart(UriPartial.Authority);

        TestContext.Progress.WriteLine($"[auth] Navigating to {FrontendUrl}/#/dashboard");
        await page.GotoAsync($"{FrontendUrl}/#/dashboard");

        bool needsLogin;
        try
        {
            await page.WaitForURLAsync(
                url => !url.StartsWith(frontendOrigin, StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = 10_000 });
            needsLogin = true;
            TestContext.Progress.WriteLine($"[auth] Redirected to Auth0 login at: {page.Url}");
        }
        catch
        {
            needsLogin = false;
            TestContext.Progress.WriteLine("[auth] Already authenticated, capturing existing session.");
        }

        if (needsLogin)
        {
            TestContext.Progress.WriteLine("[auth] Performing Auth0 login...");
            await PerformAuth0LoginOnPageAsync(page, frontendOrigin);
            TestContext.Progress.WriteLine($"[auth] Login complete, returned to: {page.Url}");
        }

        string storageState = await ctx.StorageStateAsync();
        TestContext.Progress.WriteLine($"[auth] Storage state captured ({storageState.Length} bytes)");

        await ctx.CloseAsync();
        return storageState;
    }

    private async Task PerformAuth0LoginAsync(string frontendOrigin) =>
        await PerformAuth0LoginOnPageAsync(Page, frontendOrigin);

    private static async Task PerformAuth0LoginOnPageAsync(IPage page, string frontendOrigin)
    {
        string username = Environment.GetEnvironmentVariable("VERTICALSLICE_E2E_USERNAME") ?? "e2e-test@test.com";
        string password = Environment.GetEnvironmentVariable("VERTICALSLICE_E2E_PASSWORD") ?? "Password123!";

        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        ILocator emailInput = page.Locator("input[name='username'], input[name='email'], input[type='email']");
        await Expect(emailInput.First).ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 30_000 });
        await emailInput.First.FillAsync(username);

        ILocator passwordInput = page.Locator("input[name='password'], input[type='password']");
        await Expect(passwordInput.First)
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = 30_000 });
        await passwordInput.First.FillAsync(password);

        ILocator submit = page.Locator("button[type='submit']");
        await submit.First.ClickAsync();

        // Handle possible consent/continue step.
        ILocator maybeContinue =
            page.Locator("button:has-text('Accept'), button:has-text('Authorize'), button:has-text('Continue')");
        if (await maybeContinue.CountAsync() > 0)
        {
            try
            {
                await maybeContinue.First.ClickAsync(new LocatorClickOptions { Timeout = 2_000 });
            }
            catch
            {
                /* ignore */
            }
        }

        await page.WaitForURLAsync(
            url => url.StartsWith(frontendOrigin, StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 60_000 });

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
