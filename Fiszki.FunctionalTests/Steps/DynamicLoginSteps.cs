using Fiszki.FunctionalTests.Abstractions;
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
        
        // Enter credentials and login
        await LoginPage.EnterEmailAsync(testUserEmail);
        await LoginPage.EnterPasswordAsync(testUserPassword);
        await LoginPage.ClickLoginAsync();
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
