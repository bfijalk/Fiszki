using Microsoft.Playwright;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public class RegisterPage : BasePage, IRegisterPage
{
    public RegisterPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateAsync()
    {
        // Navigate to home first, then try multiple strategies to reach register page
        await NavigateToAsync(TestConstants.Routes.Home);
        await WaitAsync();
        
        // Strategy 1: Direct register link in navigation
        var registerLink = Page.Locator(TestConstants.Selectors.Navigation).GetByText(TestConstants.Labels.Register);
        if (await registerLink.CountAsync() > 0)
        {
            await registerLink.ClickAsync();
        }
        else
        {
            // Strategy 2: Go through login page to find "Create Account" link
            await NavigateToLoginThenRegister();
        }
        
        await WaitAsync();
    }

    public async Task EnterEmailAsync(string email) => 
        await Page.FillAsync(TestConstants.Selectors.EmailInput, email);

    public async Task EnterPasswordAsync(string password) => 
        await Page.FillAsync(TestConstants.Selectors.PasswordInput, password);

    public async Task EnterConfirmPasswordAsync(string confirmPassword) => 
        await Page.FillAsync(TestConstants.Selectors.PasswordConfirmInput, confirmPassword);

    public ILocator PasswordMismatchMessage => Page.Locator(TestConstants.Selectors.PasswordMismatchMessage);

    private async Task NavigateToLoginThenRegister()
    {
        var loginLink = Page.Locator(TestConstants.Selectors.Navigation).GetByText(TestConstants.Labels.Login);
        if (await loginLink.CountAsync() > 0)
        {
            await loginLink.ClickAsync();
            await WaitAsync(TestConstants.Timeouts.DefaultWaitMs / 2);
            
            // Look for "Create Account" button on login page
            var createAccountBtn = Page.GetByText(TestConstants.Labels.CreateAccount);
            if (await createAccountBtn.CountAsync() > 0)
            {
                await createAccountBtn.ClickAsync();
                return;
            }
        }
        
        // Fallback - try direct navigation
        await NavigateToAsync(TestConstants.Routes.Register);
    }
}
