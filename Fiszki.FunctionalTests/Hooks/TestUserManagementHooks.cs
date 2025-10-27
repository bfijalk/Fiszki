using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Services;
using Fiszki.FunctionalTests.Support;
using Microsoft.Extensions.Configuration;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class TestUserManagementHooks
{
    private readonly ScenarioContext _scenarioContext;
    private TestUserManagementService? _testUserService;
    
    public TestUserManagementHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private TestUserManagementService TestUserService
    {
        get
        {
            if (_testUserService == null)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("FiszkiDatabase") 
                    ?? throw new InvalidOperationException("FiszkiDatabase connection string not found");

                _testUserService = new TestUserManagementService(connectionString);
            }
            return _testUserService;
        }
    }

    [BeforeScenario]
    public async Task BeforeScenario()
    {
        // Generate a unique test user email for this scenario
        var scenarioTitle = _scenarioContext.ScenarioInfo.Title;
        var testUserNumber = GetTestUserNumberFromScenario(scenarioTitle);
        var testUserEmail = $"test{testUserNumber}@test.pl";
        var testUserPassword = "test123";

        Console.WriteLine($"[Test User Management] Setting up test user '{testUserEmail}' for scenario: {scenarioTitle}");

        // Ensure the test user exists
        var user = await TestUserService.EnsureTestUserExistsAsync(testUserEmail, testUserPassword);
        
        // Clear any existing flashcards for this user to ensure clean state
        await TestUserService.ClearFlashcardsForUserAsync(testUserEmail);

        // Store the test user credentials in scenario context for use in steps
        _scenarioContext[TestContextKeys.TestUserEmail] = testUserEmail;
        _scenarioContext[TestContextKeys.TestUserPassword] = testUserPassword;
        _scenarioContext[TestContextKeys.TestUserId] = user.Id.ToString();

        Console.WriteLine($"[Test User Management] Test user '{testUserEmail}' ready for scenario");
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        if (_scenarioContext.ContainsKey(TestContextKeys.TestUserEmail))
        {
            var testUserEmail = _scenarioContext[TestContextKeys.TestUserEmail].ToString();
            
            // Only clean up if the scenario failed or if we want to always clean up
            if (_scenarioContext.TestError != null)
            {
                Console.WriteLine($"[Test User Management] Scenario failed. Preserving flashcards for debugging for user '{testUserEmail}'.");
                Console.WriteLine($"[Test User Management] Error: {_scenarioContext.TestError.Message}");
            }
            else
            {
                // Scenario passed - clean up the flashcards
                Console.WriteLine($"[Test User Management] Scenario passed. Cleaning up flashcards for user '{testUserEmail}'.");
                await TestUserService.ClearFlashcardsForUserAsync(testUserEmail!);
            }
        }
    }

    /// <summary>
    /// Determines the test user number based on the scenario title
    /// This ensures each scenario gets a unique user
    /// </summary>
    private static int GetTestUserNumberFromScenario(string scenarioTitle)
    {
        // Map scenario titles to specific user numbers for consistency
        var scenarioUserMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // FlashcardsUI.feature scenarios
            { "View flashcard statistics and filters", 1 },
            { "Toggle between card and list view", 2 },
            { "Create a manual flashcard", 3 },
            { "Create manual flashcard with validation error", 4 },
            { "Flip flashcards in card view", 5 },
            { "Delete a flashcard with confirmation", 6 },
            { "Empty flashcards state", 7 },
            
            // FlashcardCreation.feature scenarios
            { "Login and create flashcards from source text", 8 },
            
            // FlashcardGeneration.feature scenarios
            { "Generate flashcards and validate they appear on generation page", 9 },
            
            // Login.feature scenarios
            { "Login button disabled when fields empty", 10 },
            { "Login button enabled when both fields filled", 11 },
            
            // Navigation.feature scenarios
            { "Nav shows Login link when not authenticated", 12 },
            
            // Register.feature scenarios
            { "Password mismatch shows message", 13 }
        };

        if (scenarioUserMap.TryGetValue(scenarioTitle, out var userNumber))
        {
            return userNumber;
        }

        // Fallback: generate a number based on hash of scenario title
        // This ensures consistency across test runs for the same scenario
        var hash = scenarioTitle.GetHashCode();
        return Math.Abs(hash % 100) + 14; // Start from 14 to avoid conflicts with predefined numbers
    }
}
