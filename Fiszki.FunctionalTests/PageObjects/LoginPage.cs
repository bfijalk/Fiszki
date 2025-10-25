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
        // Try multiple approaches to find the email field
        ILocator? emailElement = null;
        
        // First try the exact selector from the working test
        try
        {
            emailElement = Page.GetByRole(AriaRole.Textbox, new() { Name = "Email*" });
            if (await emailElement.CountAsync() > 0)
            {
                await emailElement.FillAsync(email);
                return;
            }
        }
        catch
        {
            // Continue to fallback options
        }

        // Try alternative role-based selectors
        var roleBasedSelectors = new[]
        {
            () => Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }),
            () => Page.GetByRole(AriaRole.Textbox, new() { NameRegex = new System.Text.RegularExpressions.Regex(@".*[Ee]mail.*") }),
            () => Page.GetByLabel("Email"),
            () => Page.GetByLabel("Email*"),
            () => Page.GetByPlaceholder("Email"),
            () => Page.GetByPlaceholder("Enter your email")
        };

        foreach (var selectorFunc in roleBasedSelectors)
        {
            try
            {
                emailElement = selectorFunc();
                if (await emailElement.CountAsync() > 0)
                {
                    await emailElement.FillAsync(email);
                    return;
                }
            }
            catch
            {
                // Continue to next approach
            }
        }

        // Final fallback to CSS selectors
        var cssSelectors = new[]
        {
            "input[type='email']",
            "input[name*='email']",
            "input[id*='email']",
            "input[placeholder*='email']",
            "input:first-of-type"
        };

        foreach (var selector in cssSelectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                await element.First.FillAsync(email);
                return;
            }
        }

        throw new InvalidOperationException("Could not find email input field with any of the attempted selectors");
    }

    public async Task EnterPasswordAsync(string password)
    {
        // Try multiple approaches to find the password field
        ILocator? passwordElement = null;
        
        // First try the exact selector from the working test
        try
        {
            passwordElement = Page.GetByRole(AriaRole.Textbox, new() { Name = "Password*" });
            if (await passwordElement.CountAsync() > 0)
            {
                await passwordElement.FillAsync(password);
                return;
            }
        }
        catch
        {
            // Continue to fallback options
        }

        // Try alternative role-based selectors
        var roleBasedSelectors = new[]
        {
            () => Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }),
            () => Page.GetByRole(AriaRole.Textbox, new() { NameRegex = new System.Text.RegularExpressions.Regex(@".*[Pp]assword.*") }),
            () => Page.GetByLabel("Password"),
            () => Page.GetByLabel("Password*"),
            () => Page.GetByPlaceholder("Password"),
            () => Page.GetByPlaceholder("Enter your password")
        };

        foreach (var selectorFunc in roleBasedSelectors)
        {
            try
            {
                passwordElement = selectorFunc();
                if (await passwordElement.CountAsync() > 0)
                {
                    await passwordElement.FillAsync(password);
                    return;
                }
            }
            catch
            {
                // Continue to next approach
            }
        }

        // Final fallback to CSS selectors
        var cssSelectors = new[]
        {
            "input[type='password']",
            "input[name*='password']",
            "input[id*='password']",
            "input[placeholder*='password']",
            "input:nth-of-type(2)"
        };

        foreach (var selector in cssSelectors)
        {
            var element = Page.Locator(selector);
            if (await element.CountAsync() > 0)
            {
                await element.First.FillAsync(password);
                return;
            }
        }

        throw new InvalidOperationException("Could not find password input field with any of the attempted selectors");
    }

    public async Task<bool> IsLoginButtonDisabledAsync() => await LoginButton.IsDisabledAsync();

    public async Task<string?> GetErrorMessageAsync()
    {
        var alert = Page.Locator(TestConstants.Selectors.Alert);
        if (await alert.CountAsync() == 0) return null;
        return await alert.First.InnerTextAsync();
    }
}
