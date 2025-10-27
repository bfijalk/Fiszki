using Microsoft.Playwright;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public class LoginPage : BasePage, ILoginPage
{
    public LoginPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    // Expose the Page property for step access
    public IPage PageInstance => Page;

    public async Task NavigateAsync()
    {
        // Navigate to home page first, then click the Login link (like in the working test)
        await NavigateToAsync(TestConstants.Routes.Home);
        await WaitAsync();
        
        // Click the Login link from the navigation
        var loginLink = Page.GetByRole(AriaRole.Link, new() { Name = "Login" });
        await loginLink.ClickAsync();
        
        // Wait for the login page to load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitAsync();
    }

    public ILocator LoginButton => Page.GetByRole(AriaRole.Button, new() { Name = TestConstants.Labels.Login });

    public async Task ClickLoginAsync() 
    {
        // Wait for the login button to be visible first
        await LoginButton.WaitForAsync(new() 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
        
        // Wait for the button to be enabled (not disabled) - this is when it turns purple/blue
        var maxWaitTime = TimeSpan.FromMilliseconds(TestConstants.Timeouts.NavigationTimeoutMs);
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < maxWaitTime)
        {
            var isDisabled = await LoginButton.IsDisabledAsync();
            if (!isDisabled)
            {
                // Button is enabled and should be purple/blue - safe to click
                await LoginButton.ClickAsync();
                return;
            }
            
            // Wait a bit before checking again
            await Page.WaitForTimeoutAsync(TestConstants.Timeouts.FormValidationWaitMs);
        }
        
        // If we get here, the button never became enabled
        throw new TimeoutException("Login button remained disabled (grayed out) after waiting. It should turn purple/blue when enabled.");
    }

    public async Task EnterEmailAsync(string email)
    {
        // MudBlazor MudTextField generates specific HTML structure
        // Try MudBlazor-specific selectors first
        var mudSelectors = new[]
        {
            () => Page.GetByLabel("Email"),
            () => Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }),
            () => Page.Locator("input[type='email']"),
            () => Page.Locator(".mud-input-control input[type='email']"),
            () => Page.Locator(".mud-text-field input").First,
            () => Page.Locator("input").Filter(new() { HasText = "" }).First, // First visible input
        };

        foreach (var selectorFunc in mudSelectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0 && await element.IsVisibleAsync())
                {
                    await element.FillAsync(email);
                    return;
                }
            }
            catch
            {
                // Continue to next approach
            }
        }

        throw new InvalidOperationException("Could not find email input field. The page might not be loaded correctly or the selectors need updating for MudBlazor components.");
    }

    public async Task EnterPasswordAsync(string password)
    {
        // MudBlazor MudTextField generates specific HTML structure
        var mudSelectors = new[]
        {
            () => Page.GetByLabel("Password"),
            () => Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }),
            () => Page.Locator("input[type='password']"),
            () => Page.Locator(".mud-input-control input[type='password']"),
            () => Page.Locator(".mud-text-field input").Nth(1), // Second input field (password)
        };

        foreach (var selectorFunc in mudSelectors)
        {
            try
            {
                var element = selectorFunc();
                if (await element.CountAsync() > 0 && await element.IsVisibleAsync())
                {
                    await element.FillAsync(password);
                    return;
                }
            }
            catch
            {
                // Continue to next approach
            }
        }

        throw new InvalidOperationException("Could not find password input field. The page might not be loaded correctly or the selectors need updating for MudBlazor components.");
    }

    public async Task<bool> IsLoginButtonDisabledAsync() => await LoginButton.IsDisabledAsync();

    public async Task<string?> GetErrorMessageAsync()
    {
        var alert = Page.Locator(TestConstants.Selectors.Alert);
        if (await alert.CountAsync() == 0) return null;
        return await alert.First.InnerTextAsync();
    }
}
