using Microsoft.Playwright;
using TechTalk.SpecFlow;
using System.Net.Http;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class PlaywrightHooks
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static readonly HttpClient _http = new();
    private static string? _baseUrl;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        // Determine base URL (external app expected to be already running)
        _baseUrl = (Environment.GetEnvironmentVariable("FISZKI_BASE_URL") ?? "http://localhost:5290").TrimEnd('/');
        Console.WriteLine($"[App] Using existing application at {_baseUrl}");

        // Optional: wait briefly for health (do not fail entire run early; just warn)
        try
        {
            await WaitForHealthAsync(_baseUrl + "/", TimeSpan.FromSeconds(10));
            Console.WriteLine("[App] Health check succeeded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[App] Warning: health check failed: {ex.Message}");
        }

        _playwright = await Playwright.CreateAsync();
        var headlessEnv = Environment.GetEnvironmentVariable("PW_HEADLESS");
        bool headless = headlessEnv != null && headlessEnv.Equals("true", StringComparison.OrdinalIgnoreCase);
        var slowMoEnv = Environment.GetEnvironmentVariable("PW_SLOWMO");
        int slowMo = 0;
        if (int.TryParse(slowMoEnv, out var parsed)) slowMo = parsed;

        Console.WriteLine($"[Playwright] Launching Chromium. Headless={headless}, SlowMo={slowMo}ms");
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless,
            SlowMo = slowMo > 0 ? slowMo : null
        });
    }

    private static async Task WaitForHealthAsync(string url, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var resp = await _http.GetAsync(url, cts.Token);
                if (resp.IsSuccessStatusCode)
                    return;
            }
            catch { /* ignore until timeout */ }
            await Task.Delay(500);
        }
        throw new InvalidOperationException($"App did not become healthy at {url} within {timeout}.");
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        Console.WriteLine("[Playwright] Closing browser.");
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
        // Do NOT stop the application; it's external/blackbox.
    }

    [BeforeScenario]
    public async Task BeforeScenario(ScenarioContext scenarioContext)
    {
        if (_browser == null) throw new InvalidOperationException("Browser not initialized in BeforeTestRun.");
        if (_baseUrl == null) throw new InvalidOperationException("Base URL not established.");
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        var page = await context.NewPageAsync();
        scenarioContext[Support.TestContextKeys.Page] = page;
        scenarioContext[Support.TestContextKeys.BaseUrl] = _baseUrl;
        Console.WriteLine($"[Scenario] Using BaseUrl={_baseUrl}");
    }

    [AfterScenario]
    public async Task AfterScenario(ScenarioContext scenarioContext)
    {
        if (scenarioContext.TryGetValue(Support.TestContextKeys.Page, out IPage? page) && page != null)
        {
            var ctx = page.Context;
            Console.WriteLine("[Scenario] Closing page/context.");
            await ctx.CloseAsync();
        }
    }
}
