using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using FluentAssertions;
using Microsoft.Playwright;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

[Binding]
public class LoginSteps : BaseSteps
{
    private ILoginPage? _loginPage;

    public LoginSteps(ScenarioContext scenarioContext) : base(scenarioContext) { }

    private ILoginPage LoginPage => _loginPage ??= GetPage<LoginPage>();

    [Given("I am on the Login page")]
    public async Task GivenIAmOnLoginPage()
    {
        await LoginPage.NavigateAsync();
    }

    [When("I click the Login button")]
    [When("I click Login")]
    public async Task WhenIClickLogin()
    {
        await LoginPage.ClickLoginAsync();
    }

    // New step to handle post-login redirect verification
    [Then("I should be redirected to the Flashcard Generation page")]
    [When("I should be redirected to the Flashcard Generation page")]
    public async Task ThenIShouldBeRedirectedToFlashcardGenerationPage()
    {
        // Get the page instance through the LoginPage
        var loginPage = (LoginPage)LoginPage;
        var pageInstance = loginPage.PageInstance;
        
        // Wait for the automatic redirect to the generate page after successful login
        // Instead of just checking URL, also verify the page content is loaded
        await pageInstance.WaitForURLAsync($"**{TestConstants.Routes.Generate}*", new()
        {
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
        
        // Wait for the "Generate Flashcards" heading to appear, confirming we're on the right page
        var generateHeading = pageInstance.GetByRole(AriaRole.Heading, new() { Name = "Generate Flashcards", Exact = true });
        await generateHeading.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
        
        // Additional wait to ensure the page is fully loaded
        await pageInstance.WaitForLoadStateAsync(LoadState.NetworkIdle, new()
        {
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
    }

    [When("I enter email \"(.*)\"")]
    [Given("I enter email \"(.*)\"")]
    public async Task WhenIEnterEmail(string email)
    {
        await LoginPage.EnterEmailAsync(email);
    }

    [When("I enter password \"(.*)\"")]
    [Given("I enter password \"(.*)\"")]
    public async Task WhenIEnterPassword(string password)
    {
        await LoginPage.EnterPasswordAsync(password);
    }

    [Then("the Login button should be disabled")]
    public async Task ThenLoginButtonShouldBeDisabled()
    {
        var isDisabled = await LoginPage.IsLoginButtonDisabledAsync();
        isDisabled.Should().BeTrue();
    }

    [Then("the Login button should be enabled")]
    public async Task ThenLoginButtonShouldBeEnabled()
    {
        // Wait briefly for reactive state update
        await Task.Delay(TestConstants.Timeouts.FormValidationWaitMs);
        var isDisabled = await LoginPage.IsLoginButtonDisabledAsync();
        isDisabled.Should().BeFalse();
    }
}
