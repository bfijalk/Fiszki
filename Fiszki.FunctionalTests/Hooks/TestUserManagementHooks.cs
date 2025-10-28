using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Services;
using Fiszki.FunctionalTests.Support;
using Microsoft.Extensions.Configuration;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class TestUserManagementHooks
{
    private readonly ScenarioContext _scenarioContext;
    
    public TestUserManagementHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    // DISABLED: This hook is disabled in favor of SeededTestUserHooks for in-memory database testing
    // [BeforeScenario]
    public Task BeforeScenario()
    {
        Console.WriteLine("[Test User Management] DISABLED: Using seeded test data instead of dynamic user creation");
        return Task.CompletedTask;
    }

    [AfterScenario]
    public Task AfterScenario()
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
                Console.WriteLine($"[Test User Management] Scenario passed. No cleanup needed for seeded data user '{testUserEmail}'.");
            }
        }
        return Task.CompletedTask;
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
