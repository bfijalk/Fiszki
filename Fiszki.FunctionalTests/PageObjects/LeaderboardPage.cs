using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.Constants;

namespace Fiszki.FunctionalTests.PageObjects;

/// <summary>One row of the leaderboard table as shown on the page.</summary>
public record LeaderboardRowInfo(int Position, string User, int Points, bool IsCurrentUser, int Flashcards, int Archived, int LearningProgress);

/// <summary>What the motivation banner currently says about the logged-in user.</summary>
public record LeaderboardBannerInfo(
    int Position,
    int Points,
    bool IsLeader,
    string? RivalText,
    int? SamePointsCount,
    string FullText,
    int Flashcards,
    int Archived,
    int LearningProgress);

public class LeaderboardPage : BasePage, ILeaderboardPage
{
    private const string MenuItemTestId = "nav-leaderboard";

    public LeaderboardPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    private ILocator Banner => Page.GetByTestId("leaderboard-banner");
    private ILocator Rows => Page.GetByTestId("leaderboard-row");
    private ILocator NextButton => Page.GetByTestId("leaderboard-next");
    private ILocator PreviousButton => Page.GetByTestId("leaderboard-prev");
    private ILocator PageIndicator => Page.GetByTestId("leaderboard-page-indicator");

    /// <summary>
    /// Opens the leaderboard through the side menu. A full page load would drop the in-memory login,
    /// so the page is never opened by URL.
    /// </summary>
    public async Task NavigateAsync()
    {
        await Page.GetByTestId(MenuItemTestId).ClickAsync();
        await WaitUntilLoadedAsync();
    }

    public async Task WaitUntilLoadedAsync()
    {
        await Banner.WaitForAsync(new()
        {
            State = WaitForSelectorState.Visible,
            Timeout = TestConstants.Timeouts.NavigationTimeoutMs
        });
    }

    /// <summary>Opens the page by URL. Only meant for visitors who are not logged in.</summary>
    public async Task OpenByUrlAsync() => await NavigateToAsync(TestConstants.Routes.Leaderboard);

    public async Task<bool> WasRedirectedToLoginAsync()
    {
        try
        {
            await Page.WaitForURLAsync("**/login**", new() { Timeout = TestConstants.Timeouts.NavigationTimeoutMs });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>Waits for the menu item to appear, for example while login is still completing.</summary>
    public async Task<bool> WaitForMenuItemAsync()
    {
        try
        {
            await Page.GetByTestId(MenuItemTestId).WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = TestConstants.Timeouts.LoginTimeoutMs
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    public async Task<bool> IsMenuItemVisibleAsync()
    {
        var item = Page.GetByTestId(MenuItemTestId);
        return await item.CountAsync() > 0 && await item.IsVisibleAsync();
    }

    public async Task<bool> IsLeaderboardPageShownAsync()
    {
        return Page.Url.TrimEnd('/').EndsWith(TestConstants.Routes.Leaderboard, StringComparison.OrdinalIgnoreCase)
               && await Page.GetByTestId("leaderboard-page").CountAsync() > 0;
    }

    public string CurrentUrl => Page.Url;

    public async Task<IReadOnlyList<LeaderboardRowInfo>> GetRowsAsync()
    {
        var count = await Rows.CountAsync();
        var result = new List<LeaderboardRowInfo>(count);
        for (var i = 0; i < count; i++)
        {
            var row = Rows.Nth(i);
            var position = int.Parse((await row.GetByTestId("leaderboard-position").InnerTextAsync()).Trim());
            var user = (await row.GetByTestId("leaderboard-user").InnerTextAsync()).Trim();
            var points = int.Parse((await row.GetByTestId("leaderboard-points").InnerTextAsync()).Trim());
            var isCurrent = await row.GetAttributeAsync("data-current-user") == "true";
            var flashcards = int.Parse((await row.GetByTestId("leaderboard-flashcards").InnerTextAsync()).Trim());
            var archived = int.Parse((await row.GetByTestId("leaderboard-archived").InnerTextAsync()).Trim());
            var progress = ParsePercent(await row.GetByTestId("leaderboard-progress").InnerTextAsync());
            result.Add(new LeaderboardRowInfo(position, user, points, isCurrent, flashcards, archived, progress));
        }
        return result;
    }

    public async Task<LeaderboardBannerInfo> GetBannerAsync()
    {
        var full = await Banner.InnerTextAsync();
        var position = await Page.GetByTestId("leaderboard-banner-position").InnerTextAsync();
        var match = Regex.Match(position, @"#(\d+)\s+with\s+(\d+)");
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not read position and points from banner text '{position}'.");
        }

        var rival = Page.GetByTestId("leaderboard-banner-rival");
        var same = Page.GetByTestId("leaderboard-banner-same-points");
        int? samePoints = null;
        if (await same.CountAsync() > 0)
        {
            var sameMatch = Regex.Match(await same.InnerTextAsync(), @"(\d+)\s+rival");
            samePoints = sameMatch.Success ? int.Parse(sameMatch.Groups[1].Value) : null;
        }

        return new LeaderboardBannerInfo(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            await Page.GetByTestId("leaderboard-banner-leader").CountAsync() > 0,
            await rival.CountAsync() > 0 ? (await rival.InnerTextAsync()).Trim() : null,
            samePoints,
            full.Trim(),
            int.Parse((await Page.GetByTestId("leaderboard-banner-flashcards").InnerTextAsync()).Trim()),
            int.Parse((await Page.GetByTestId("leaderboard-banner-archived").InnerTextAsync()).Trim()),
            ParsePercent(await Page.GetByTestId("leaderboard-banner-progress").InnerTextAsync()));
    }

    public async Task<(int Page, int TotalPages)> GetPageIndicatorAsync()
    {
        var match = Regex.Match(await PageIndicator.InnerTextAsync(), @"Page\s+(\d+)\s+of\s+(\d+)");
        if (!match.Success)
        {
            throw new InvalidOperationException("Could not read the page indicator.");
        }
        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    public Task<bool> IsNextDisabledAsync() => NextButton.IsDisabledAsync();

    public Task<bool> IsPreviousDisabledAsync() => PreviousButton.IsDisabledAsync();

    public async Task ClickNextAsync()
    {
        var (current, _) = await GetPageIndicatorAsync();
        await NextButton.ClickAsync();
        await WaitForPageNumberAsync(current + 1);
    }

    public async Task ClickPreviousAsync()
    {
        var (current, _) = await GetPageIndicatorAsync();
        await PreviousButton.ClickAsync();
        await WaitForPageNumberAsync(current - 1);
    }

    public async Task<string> GetVisibleTextAsync() => await Page.GetByTestId("leaderboard-page").InnerTextAsync();

    private static int ParsePercent(string text) => int.Parse(text.Replace("%", string.Empty).Trim());

    private async Task WaitForPageNumberAsync(int expected)
    {
        await Page.WaitForFunctionAsync(
            "n => document.querySelector('[data-testid=leaderboard-page-indicator]')?.textContent.includes('Page ' + n + ' of')",
            expected,
            new() { Timeout = TestConstants.Timeouts.NavigationTimeoutMs });
    }
}
