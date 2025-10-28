using TechTalk.SpecFlow;
using Fiszki.FunctionalTests.Support;

namespace Fiszki.FunctionalTests.Hooks;

[Binding]
public class SeededTestUserHooks
{
    private readonly ScenarioContext _scenarioContext;
    
    public SeededTestUserHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void SetupSeededTestUser()
    {
        var scenarioTitle = _scenarioContext.ScenarioInfo.Title;
        var tags = _scenarioContext.ScenarioInfo.Tags;
        
        // Assign appropriate seeded test user based on scenario requirements
        if (tags.Contains("empty3") || scenarioTitle.Contains("empty3", StringComparison.OrdinalIgnoreCase))
        {
            // Use the third empty user for guaranteed empty state testing
            _scenarioContext[TestContextKeys.TestUserEmail] = "empty3@example.com";
            _scenarioContext[TestContextKeys.TestUserPassword] = "empty3123";
            _scenarioContext[TestContextKeys.TestUserType] = "empty3";
            Console.WriteLine($"[Seeded User] Assigned empty3 user for scenario: {scenarioTitle}");
        }
        else if (tags.Contains("empty2") || scenarioTitle.Contains("empty2", StringComparison.OrdinalIgnoreCase))
        {
            // Use the second empty user for additional empty state testing
            _scenarioContext[TestContextKeys.TestUserEmail] = "empty2@example.com";
            _scenarioContext[TestContextKeys.TestUserPassword] = "empty2123";
            _scenarioContext[TestContextKeys.TestUserType] = "empty2";
            Console.WriteLine($"[Seeded User] Assigned empty2 user for scenario: {scenarioTitle}");
        }
        else if (tags.Contains("empty") || scenarioTitle.Contains("empty", StringComparison.OrdinalIgnoreCase))
        {
            // Use the third empty user for main empty state scenarios
            _scenarioContext[TestContextKeys.TestUserEmail] = "empty3@example.com";
            _scenarioContext[TestContextKeys.TestUserPassword] = "empty3123";
            _scenarioContext[TestContextKeys.TestUserType] = "empty3";
            Console.WriteLine($"[Seeded User] Assigned empty3 user for scenario: {scenarioTitle}");
        }
        else if (tags.Contains("admin") || scenarioTitle.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            // Use admin user for admin-specific scenarios
            _scenarioContext[TestContextKeys.TestUserEmail] = "admin@example.com";
            _scenarioContext[TestContextKeys.TestUserPassword] = "admin123";
            _scenarioContext[TestContextKeys.TestUserType] = "admin";
            Console.WriteLine($"[Seeded User] Assigned admin user for scenario: {scenarioTitle}");
        }
        else if (tags.Contains("demo") || scenarioTitle.Contains("demo", StringComparison.OrdinalIgnoreCase))
        {
            // Use demo user for scenarios that need some existing flashcards
            _scenarioContext[TestContextKeys.TestUserEmail] = "demo@example.com";
            _scenarioContext[TestContextKeys.TestUserPassword] = "demo123";
            _scenarioContext[TestContextKeys.TestUserType] = "demo";
            Console.WriteLine($"[Seeded User] Assigned demo user for scenario: {scenarioTitle}");
        }
        else
        {
            // Default to test user for most scenarios
            _scenarioContext[TestContextKeys.TestUserEmail] = "test@example.com";
            _scenarioContext[TestContextKeys.TestUserPassword] = "password123";
            _scenarioContext[TestContextKeys.TestUserType] = "test";
            Console.WriteLine($"[Seeded User] Assigned test user for scenario: {scenarioTitle}");
        }
    }
}
