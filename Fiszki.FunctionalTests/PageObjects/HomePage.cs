using Microsoft.Playwright;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public class HomePage : BasePage
{
    public HomePage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateAsync() => await NavigateToAsync(TestConstants.Routes.Home);
}
