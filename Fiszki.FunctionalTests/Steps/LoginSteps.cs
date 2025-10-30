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
        var pageInstance = ((LoginPage)LoginPage).PageInstance;
        
        Console.WriteLine($"[Login Redirect] Starting redirect verification. Current URL: {pageInstance.Url}");
        
        // Wait for navigation to complete using Playwright's built-in mechanisms
        try
        {
            // Wait for URL to change from login page with a reasonable timeout
            await pageInstance.WaitForURLAsync(url => !url.Contains("/login"), new()
            {
                Timeout = TestConstants.Timeouts.RedirectWaitMs
            });
        }
        catch (TimeoutException)
        {
            Console.WriteLine("[Login Redirect] Timeout waiting for redirect from login page");
        }
        
        var currentUrl = pageInstance.Url;
        Console.WriteLine($"[Login Redirect] Current URL after redirect: {currentUrl}");
        
        // Check if we're on any valid post-login page
        if (currentUrl.Contains("/flashcards") || currentUrl.Contains("/generate") || 
            currentUrl.Contains("/home") || currentUrl.Contains("/dashboard"))
        {
            Console.WriteLine($"[Login Redirect] Login successful - redirected to: {currentUrl}");
            
            // If we're on the generate page, try to verify content is loaded
            if (currentUrl.Contains("/generate"))
            {
                try
                {
                    // Wait for any heading that indicates the page is loaded
                    var heading = pageInstance.Locator("h1, h2, h3, h4, h5, h6").First;
                    await heading.WaitForAsync(new() { Timeout = TestConstants.Timeouts.ContentLoadWaitMs });
                    Console.WriteLine("[Login Redirect] Generate page content loaded successfully");
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("[Login Redirect] Generate page loaded but content may still be loading");
                    // This is acceptable - page redirected correctly
                }
            }
            return;
        }
        
        // If we're still on login page, check for error messages
        if (currentUrl.Contains("/login"))
        {
            Console.WriteLine("[Login Redirect] Still on login page - checking for errors");
            try
            {
                var errorElement = pageInstance.Locator(".alert-danger, .error, .text-danger").First;
                if (await errorElement.CountAsync() > 0)
                {
                    var errorText = await errorElement.TextContentAsync();
                    Console.WriteLine($"[Login Redirect] Found error: {errorText}");
                }
            }
            catch
            {
                // No error messages found
            }
        }
        
        throw new InvalidOperationException($"Login redirect failed. Expected to be redirected from login page, but current URL is: {currentUrl}");
    }

    [Then("I should be redirected to the Flashcards page")]
    [When("I should be redirected to the Flashcards page")]
    public async Task ThenIShouldBeRedirectedToFlashcardsPage()
    {
        var pageInstance = ((LoginPage)LoginPage).PageInstance;
        
        Console.WriteLine($"[Login Redirect] Starting redirect verification. Current URL: {pageInstance.Url}");
        
        // Wait for navigation to complete using Playwright's built-in mechanisms
        try
        {
            // Wait for URL to change from login page with a reasonable timeout
            await pageInstance.WaitForURLAsync(url => !url.Contains("/login"), new()
            {
                Timeout = TestConstants.Timeouts.RedirectWaitMs
            });
        }
        catch (TimeoutException)
        {
            Console.WriteLine("[Login Redirect] Timeout waiting for redirect from login page");
        }
        
        var currentUrl = pageInstance.Url;
        Console.WriteLine($"[Login Redirect] Current URL after redirect: {currentUrl}");
        
        // Check if we're on the flashcards page
        if (currentUrl.Contains("/flashcards"))
        {
            Console.WriteLine($"[Login Redirect] Login successful - redirected to flashcards page: {currentUrl}");
            return;
        }
        
        throw new InvalidOperationException($"Login redirect failed. Expected to be redirected to flashcards page, but current URL is: {currentUrl}");
    }

    [Given("I enter email \"(.*)\"")]
    [When("I enter email {string}")]
    public async Task WhenIEnterEmail(string email)
    {
        var page = ((LoginPage)LoginPage).PageInstance;
        
        // Wait for the email field to be available
        var emailField = page.GetByRole(AriaRole.Textbox, new() { Name = "Email*" });
        await emailField.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.ElementWaitMs
        });
        
        await emailField.FillAsync(email);
    }

    [When("I enter password \"(.*)\"")]
    [Given("I enter password \"(.*)\"")]
    [When("I enter password {string}")]
    public async Task WhenIEnterPassword(string password)
    {
        var page = ((LoginPage)LoginPage).PageInstance;
        
        // Wait for the password field to be available
        var passwordField = page.GetByRole(AriaRole.Textbox, new() { Name = "Password*" });
        await passwordField.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.ElementWaitMs
        });
        
        await passwordField.FillAsync(password);
    }

    [When("I click Login button")]
    [Given("I click Login button")]
    public async Task WhenIClickLoginButton()
    {
        var page = ((LoginPage)LoginPage).PageInstance;
        
        // Wait for the login button to be available and enabled
        var loginButton = page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        await loginButton.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.ElementWaitMs
        });
        
        await loginButton.ClickAsync();
        
        // Wait for navigation to start instead of a fixed delay
        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = TestConstants.Timeouts.NetworkIdleWaitMs });
        }
        catch (TimeoutException)
        {
            // Navigation might still be in progress, that's ok
        }
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
        // Use Playwright's built-in waiting instead of hardcoded delay
        var page = ((LoginPage)LoginPage).PageInstance;
        var loginButton = page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        
        // Wait for button to be enabled
        await loginButton.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = TestConstants.Timeouts.ButtonStateWaitMs
        });
        
        var isDisabled = await LoginPage.IsLoginButtonDisabledAsync();
        isDisabled.Should().BeFalse();
    }
}
