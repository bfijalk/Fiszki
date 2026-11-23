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

    /// <summary>Marker returned by <see cref="GetMenuEntriesAsync"/> for a separator between menu groups.</summary>
    public const string SeparatorMarker = "---";

    /// <summary>
    /// The side menu from top to bottom in one browser call: the text of each item and
    /// <see cref="SeparatorMarker"/> for each separator.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetMenuEntriesAsync()
    {
        var entries = await _page.EvaluateAsync<string[]>(
            @"() => [...document.querySelector('nav.flex-column').children]
                       .map(e => e.tagName === 'HR' ? '" + SeparatorMarker + @"' : e.innerText.trim())");
        return entries;
    }
}
