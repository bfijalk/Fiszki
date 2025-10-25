using Microsoft.Playwright;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page ?? throw new ArgumentNullException(nameof(page));
        BaseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    protected async Task NavigateToAsync(string route)
    {
        var url = BaseUrl + route;
        await Page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
    }

    protected async Task WaitForElementAsync(string selector, int timeoutMs = TestConstants.Timeouts.DefaultWaitMs)
    {
        await Page.WaitForSelectorAsync(selector, new() { Timeout = timeoutMs });
    }

    protected async Task WaitAsync(int milliseconds = TestConstants.Timeouts.DefaultWaitMs)
    {
        await Page.WaitForTimeoutAsync(milliseconds);
    }
}
