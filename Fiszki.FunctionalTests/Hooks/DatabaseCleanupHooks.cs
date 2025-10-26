using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Services;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class DatabaseCleanupHooks
{
    private static DatabaseCleanupService? _cleanupService;
    private const string DefaultConnectionString = "Host=localhost;Port=5434;Database=fiszki_dev;Username=postgres;Password=postgres";

    [BeforeTestRun]
    public static void InitializeDatabaseCleanup()
    {
        // Get connection string from environment variable or use default
        var connectionString = Environment.GetEnvironmentVariable("FISZKI_DB_CONNECTION_STRING") ?? DefaultConnectionString;
        _cleanupService = new DatabaseCleanupService(connectionString);
        
        Console.WriteLine("[Database Cleanup] Initialized with connection string.");
    }

    [BeforeScenario]
    public static async Task BeforeScenario()
    {
        if (_cleanupService == null)
        {
            Console.WriteLine("[Database Cleanup] Service not initialized. Skipping pre-scenario cleanup.");
            return;
        }

        // Ensure test user exists before running tests
        var userExists = await _cleanupService.EnsureTestUserExistsAsync();
        if (!userExists)
        {
            Console.WriteLine("[Database Cleanup] Warning: Test user not found. Tests may fail.");
        }

        // Clean up any existing flashcards to ensure clean state
        Console.WriteLine("[Database Cleanup] Cleaning flashcards before scenario to ensure clean state.");
        await _cleanupService.ClearFlashcardsForTestUserAsync();
    }

    [AfterScenario]
    public static async Task AfterScenario(ScenarioContext scenarioContext)
    {
        if (_cleanupService == null)
        {
            Console.WriteLine("[Database Cleanup] Service not initialized. Skipping post-scenario cleanup.");
            return;
        }

        // Only clean up after successful scenarios to preserve data for debugging failed tests
        if (scenarioContext.TestError == null)
        {
            Console.WriteLine("[Database Cleanup] Scenario completed successfully. Cleaning up flashcards.");
            await _cleanupService.ClearFlashcardsForTestUserAsync();
        }
        else
        {
            Console.WriteLine("[Database Cleanup] Scenario failed. Preserving flashcards for debugging.");
            Console.WriteLine($"[Database Cleanup] Error: {scenarioContext.TestError.Message}");
        }
    }
}
