using Microsoft.Playwright;
using TechTalk.SpecFlow;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class PlaywrightHooks
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static readonly HttpClient _http = new();
    private static string? _baseUrl;
    private static IConfiguration? _configuration;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        // Load configuration from appsettings.json
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Determine base URL with priority: Environment Variable > Configuration > Default
        _baseUrl = Environment.GetEnvironmentVariable("FISZKI_BASE_URL") 
            ?? _configuration["Playwright:BaseUrl"] 
            ?? "http://localhost:5290";
        _baseUrl = _baseUrl.TrimEnd('/');
        
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
        
        // Get browser configuration with priority: Environment Variables > Configuration > Defaults
        var headlessEnv = Environment.GetEnvironmentVariable("PW_HEADLESS");
        bool headless;
        if (headlessEnv != null)
        {
            // Environment variable takes precedence
            headless = !headlessEnv.Equals("false", StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // Fall back to configuration, default to true (headless)
            bool.TryParse(_configuration["Playwright:Headless"], out headless);
            if (_configuration["Playwright:Headless"] == null) headless = true; // Default to headless
        }

        var slowMoEnv = Environment.GetEnvironmentVariable("PW_SLOWMO");
        int slowMo;
        if (int.TryParse(slowMoEnv, out var parsedSlowMo))
        {
            slowMo = parsedSlowMo;
        }
        else
        {
            if (!int.TryParse(_configuration["Playwright:SlowMotion"], out slowMo))
            {
                slowMo = 0; // Default value
            }
        }

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
        
        // Get viewport configuration from settings
        int viewportWidth = 1280; // Default
        int viewportHeight = 800; // Default
        int defaultTimeout = 7000; // Default
        
        if (_configuration != null)
        {
            if (int.TryParse(_configuration["Playwright:ViewportWidth"], out var width)) viewportWidth = width;
            if (int.TryParse(_configuration["Playwright:ViewportHeight"], out var height)) viewportHeight = height;
            if (int.TryParse(_configuration["Playwright:DefaultTimeout"], out var timeout)) defaultTimeout = timeout;
        }
        
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = viewportWidth, Height = viewportHeight }
        });
        
        // Set the timeout from configuration
        context.SetDefaultTimeout(defaultTimeout);
        
        var page = await context.NewPageAsync();
        scenarioContext[Support.TestContextKeys.Page] = page;
        scenarioContext[Support.TestContextKeys.BaseUrl] = _baseUrl;
        Console.WriteLine($"[Scenario] Using BaseUrl={_baseUrl}, Viewport={viewportWidth}x{viewportHeight}, Timeout={defaultTimeout}ms");
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
