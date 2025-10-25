using Microsoft.Playwright;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

public class NavBar
{
    private readonly IPage _page;

    public NavBar(IPage page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    public ILocator GetLink(string linkText) => 
        _page.Locator($"{TestConstants.Selectors.Navigation} .nav-link:has-text(\"{linkText}\")");

    public async Task ClickLinkAsync(string linkText)
    {
        var link = GetLink(linkText);
        await link.ClickAsync();
    }

    public async Task<bool> HasLinkAsync(string linkText)
    {
        var link = GetLink(linkText);
        return await link.CountAsync() > 0;
    }
}
