using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Services;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class DatabaseCleanupHooks
{
    private static DatabaseCleanupService? _cleanupService;
    private const string DefaultConnectionString = "Host=localhost;Port=5434;Database=fiszki_dev;Username=postgres;Password=postgres";

    // DISABLED: These hooks are disabled since we're using in-memory database with seeded data
    // [BeforeTestRun]
    public static void InitializeDatabaseCleanup()
    {
        Console.WriteLine("[Database Cleanup] DISABLED: Using in-memory database with seeded data instead");
    }

    // [BeforeScenario]
    public static Task BeforeScenario()
    {
        Console.WriteLine("[Database Cleanup] DISABLED: Using in-memory database with seeded data instead");
        return Task.CompletedTask;
    }

    // [AfterScenario]
    public static Task AfterScenario(ScenarioContext scenarioContext)
    {
        Console.WriteLine("[Database Cleanup] DISABLED: Using in-memory database with seeded data instead");
        return Task.CompletedTask;
    }
}
