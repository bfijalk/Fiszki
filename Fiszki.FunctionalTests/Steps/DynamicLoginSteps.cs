using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using Fiszki.FunctionalTests.Support;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class DynamicLoginSteps : BaseSteps
{
    private readonly ScenarioContext _scenarioContext;
    private ILoginPage? _loginPage;

    public DynamicLoginSteps(ScenarioContext scenarioContext) : base(scenarioContext) 
    {
        _scenarioContext = scenarioContext;
    }

    private ILoginPage LoginPage => _loginPage ??= GetPage<LoginPage>();

    /// <summary>
    /// Login using the dynamic test user assigned to this scenario
    /// </summary>
    [When("I login with my test user")]
    [Given("I login with my test user")]
    public async Task WhenILoginWithMyTestUser()
    {
        // Get the test user credentials from scenario context
        var testUserEmail = _scenarioContext[TestContextKeys.TestUserEmail]?.ToString()
            ?? throw new InvalidOperationException("Test user email not found in scenario context");
        var testUserPassword = _scenarioContext[TestContextKeys.TestUserPassword]?.ToString()
            ?? throw new InvalidOperationException("Test user password not found in scenario context");

        Console.WriteLine($"[Dynamic Login] Logging in with test user: {testUserEmail}");

        // Navigate to login page if not already there
        await LoginPage.NavigateAsync();
        
        // Wait a bit for the page to fully load
        await Task.Delay(TestConstants.Timeouts.DefaultWaitMs);
        
        // Enter credentials and login with extended timeouts
        await LoginPage.EnterEmailAsync(testUserEmail);
        await Task.Delay(TestConstants.Timeouts.FormValidationWaitMs); // Allow for form validation
        
        await LoginPage.EnterPasswordAsync(testUserPassword);
        await Task.Delay(TestConstants.Timeouts.FormValidationWaitMs); // Allow for form validation
        
        // Click login and wait longer for the login process to complete
        await LoginPage.ClickLoginAsync();
        
        // Wait for login processing - this is crucial for slow login responses
        await Task.Delay(TestConstants.Timeouts.LoginTimeoutMs);
        
        // Additional wait for authentication state to propagate in test environment
        Console.WriteLine("[Dynamic Login] Waiting additional time for authentication state propagation...");
        await Task.Delay(2000);
        
        // Verify we're not still on the login page after successful login
        var currentUrl = LoginPage.PageInstance.Url;
        if (currentUrl.Contains("/login"))
        {
            Console.WriteLine($"[Dynamic Login] Warning: Still on login page after login attempt. URL: {currentUrl}");
            // Wait a bit more and check again
            await Task.Delay(3000);
            currentUrl = LoginPage.PageInstance.Url;
            if (currentUrl.Contains("/login"))
            {
                throw new InvalidOperationException($"Login failed - still on login page after extended wait. Current URL: {currentUrl}");
            }
        }
        
        Console.WriteLine($"[Dynamic Login] Login process completed for user: {testUserEmail}. Final URL: {currentUrl}");
    }

    /// <summary>
    /// Enter the email of the current test user
    /// </summary>
    [When("I enter my test user email")]
    [Given("I enter my test user email")]
    public async Task WhenIEnterMyTestUserEmail()
    {
        var testUserEmail = _scenarioContext[TestContextKeys.TestUserEmail]?.ToString()
            ?? throw new InvalidOperationException("Test user email not found in scenario context");

        await LoginPage.EnterEmailAsync(testUserEmail);
    }

    /// <summary>
    /// Enter the password of the current test user
    /// </summary>
    [When("I enter my test user password")]
    [Given("I enter my test user password")]
    public async Task WhenIEnterMyTestUserPassword()
    {
        var testUserPassword = _scenarioContext[TestContextKeys.TestUserPassword]?.ToString()
            ?? throw new InvalidOperationException("Test user password not found in scenario context");

        await LoginPage.EnterPasswordAsync(testUserPassword);
    }

    /// <summary>
    /// Complete login process with current test user credentials
    /// </summary>
    [When("I complete the login process")]
    [Given("I complete the login process")]
    public async Task WhenICompleteTheLoginProcess()
    {
        await WhenIEnterMyTestUserEmail();
        await WhenIEnterMyTestUserPassword();
        await LoginPage.ClickLoginAsync();
    }
}
