using System.Text.RegularExpressions;
using Fiszki.FunctionalTests.Abstractions;
using Fiszki.FunctionalTests.PageObjects;
using Fiszki.FunctionalTests.Steps.Base;
using Fiszki.FunctionalTests.Support;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace Fiszki.FunctionalTests.Steps;

/// <summary>
/// Steps for Leaderboard.feature. They assert on the logged-in user's own row, points and banner and on
/// structural properties of the ranking, never on the absolute position of other users, because the
/// database is shared with other scenarios.
/// </summary>
[Binding]
public class LeaderboardSteps : BaseSteps
{
    private static readonly Regex MaskedUser = new(@"^\S{0,2}\*\*\*@\S+$", RegexOptions.Compiled);

    private readonly ScenarioContext _scenarioContext;
    private ILeaderboardPage? _leaderboardPage;
    private int _rememberedPoints;
    private int _rememberedFlashcards;
    private int _rememberedArchived;

    public LeaderboardSteps(ScenarioContext scenarioContext) : base(scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private ILeaderboardPage Leaderboard => _leaderboardPage ??= GetPage<LeaderboardPage>();

    // ----- access and menu -----

    [Then("I should see the Leaderboard menu item")]
    public async Task ThenIShouldSeeTheLeaderboardMenuItem()
    {
        (await Leaderboard.WaitForMenuItemAsync()).Should().BeTrue("a logged-in user sees the Leaderboard menu item");
    }

    [Then("I should not see the Leaderboard menu item")]
    public async Task ThenIShouldNotSeeTheLeaderboardMenuItem()
    {
        (await Leaderboard.IsMenuItemVisibleAsync()).Should().BeFalse("visitors do not see the Leaderboard menu item");
    }

    [When("I open the leaderboard")]
    public async Task WhenIOpenTheLeaderboard()
    {
        await Leaderboard.NavigateAsync();
    }

    [When("I open the leaderboard address without logging in")]
    public async Task WhenIOpenTheLeaderboardAddressWithoutLoggingIn()
    {
        await Leaderboard.OpenByUrlAsync();
    }

    [Then("I should be asked to log in")]
    public async Task ThenIShouldBeAskedToLogIn()
    {
        (await Leaderboard.WasRedirectedToLoginAsync()).Should().BeTrue("the leaderboard is protected like the other logged-in pages");
        (await Leaderboard.IsLeaderboardPageShownAsync()).Should().BeFalse();
    }

    [Then("I should be on the leaderboard page")]
    public async Task ThenIShouldBeOnTheLeaderboardPage()
    {
        (await Leaderboard.IsLeaderboardPageShownAsync()).Should().BeTrue($"the current URL is {Leaderboard.CurrentUrl}");
    }

    [Then("the banner should show my position and points")]
    public async Task ThenTheBannerShouldShowMyPositionAndPoints()
    {
        var banner = await Leaderboard.GetBannerAsync();
        banner.Position.Should().BeGreaterThan(0);
        banner.Points.Should().BeGreaterThanOrEqualTo(0);
    }

    [Then("the banner should still be shown")]
    public async Task ThenTheBannerShouldStillBeShown()
    {
        var banner = await Leaderboard.GetBannerAsync();
        banner.Position.Should().BeGreaterThan(0, "the banner does not depend on the page being viewed");
    }

    // ----- points -----

    [When("I remember my leaderboard points")]
    public async Task WhenIRememberMyLeaderboardPoints()
    {
        var banner = await Leaderboard.GetBannerAsync();
        _rememberedPoints = banner.Points;
        _rememberedFlashcards = banner.Flashcards;
        _rememberedArchived = banner.Archived;
    }

    [Then("my leaderboard flashcards should be {int} higher than before")]
    public async Task ThenMyLeaderboardFlashcardsShouldBeHigherThanBefore(int difference)
    {
        (await Leaderboard.GetBannerAsync()).Flashcards.Should().Be(_rememberedFlashcards + difference);
    }

    [Then("my leaderboard archived flashcards should be {int} higher than before")]
    public async Task ThenMyLeaderboardArchivedShouldBeHigherThanBefore(int difference)
    {
        (await Leaderboard.GetBannerAsync()).Archived.Should().Be(_rememberedArchived + difference);
    }

    [Then("my leaderboard flashcards and archived flashcards should be the same as before")]
    public async Task ThenMyLeaderboardCountsShouldBeTheSameAsBefore()
    {
        var banner = await Leaderboard.GetBannerAsync();
        banner.Flashcards.Should().Be(_rememberedFlashcards);
        banner.Archived.Should().Be(_rememberedArchived);
    }

    [Then("every user on the page should have points equal to flashcards plus 3 per archived flashcard")]
    public async Task ThenEveryRowShouldFollowThePointsFormula()
    {
        var rows = await Leaderboard.GetRowsAsync();
        rows.Should().NotBeEmpty();
        foreach (var row in rows)
        {
            row.Archived.Should().BeLessThanOrEqualTo(row.Flashcards, $"position {row.Position} cannot archive more than it owns");
            row.Points.Should().Be(row.Flashcards + 3 * row.Archived, $"position {row.Position} gets 1 point per flashcard and 3 extra per archived one");
        }
    }

    [Then("every user on the page should show a learning progress matching their archived and total flashcards")]
    public async Task ThenEveryRowShouldShowTheRightProgress()
    {
        foreach (var row in await Leaderboard.GetRowsAsync())
        {
            var expected = row.Flashcards == 0 ? 0 : row.Archived * 100 / row.Flashcards;
            row.LearningProgress.Should().Be(expected, $"position {row.Position} has {row.Archived} of {row.Flashcards} archived");
        }
    }

    [Then("the banner should show my own flashcards, archived flashcards and learning progress from my row")]
    public async Task ThenTheBannerShouldShowMyOwnNumbers()
    {
        var banner = await Leaderboard.GetBannerAsync();
        var me = (await ReadWholeRankingAsync()).Single(r => r.IsCurrentUser);
        banner.Flashcards.Should().Be(me.Flashcards);
        banner.Archived.Should().Be(me.Archived);
        banner.LearningProgress.Should().Be(me.LearningProgress);
        banner.Points.Should().Be(me.Points);
    }

    [Then("my leaderboard points should be {int} higher than before")]
    public async Task ThenMyLeaderboardPointsShouldBeHigherThanBefore(int difference)
    {
        (await Leaderboard.GetBannerAsync()).Points.Should().Be(_rememberedPoints + difference);
    }

    [Then("my leaderboard points should be the same as before")]
    public async Task ThenMyLeaderboardPointsShouldBeTheSameAsBefore()
    {
        (await Leaderboard.GetBannerAsync()).Points.Should().Be(_rememberedPoints);
    }

    [Then("my leaderboard points should be {int}")]
    public async Task ThenMyLeaderboardPointsShouldBe(int points)
    {
        (await Leaderboard.GetBannerAsync()).Points.Should().Be(points);
    }

    // ----- ordering and identity -----

    [Then("the points on the page should be sorted from highest to lowest")]
    public async Task ThenThePointsShouldBeSorted()
    {
        var points = (await Leaderboard.GetRowsAsync()).Select(r => r.Points).ToList();
        points.Should().BeInDescendingOrder();
    }

    [Then("the positions on the page should be unique and consecutive starting from {int}")]
    public async Task ThenThePositionsShouldBeUniqueAndConsecutive(int first)
    {
        var positions = (await Leaderboard.GetRowsAsync()).Select(r => r.Position).ToList();
        positions.Should().Equal(Enumerable.Range(first, positions.Count));
    }

    [Then("users without points should be listed after users with points")]
    public async Task ThenUsersWithoutPointsShouldBeListedLast()
    {
        var points = (await Leaderboard.GetRowsAsync()).Select(r => r.Points).ToList();
        var firstZero = points.FindIndex(p => p == 0);
        firstZero.Should().BeGreaterThanOrEqualTo(0, "the seeded data contains users without flashcards");
        points.Skip(firstZero).Should().OnlyContain(p => p == 0);
    }

    [Then("every user on the page should be shown with a masked email")]
    public async Task ThenEveryUserShouldBeMasked()
    {
        var rows = await Leaderboard.GetRowsAsync();
        rows.Should().NotBeEmpty();
        foreach (var row in rows)
        {
            var user = row.User.Replace("(you)", string.Empty).Trim();
            MaskedUser.IsMatch(user).Should().BeTrue($"'{user}' must be a masked email");
        }
    }

    [Then("the page should not show my full email")]
    public async Task ThenThePageShouldNotShowMyFullEmail()
    {
        var email = _scenarioContext[TestContextKeys.TestUserEmail]!.ToString()!;
        (await Leaderboard.GetVisibleTextAsync()).Should().NotContainEquivalentOf(email);
    }

    [Then("my own row should be highlighted")]
    public async Task ThenMyOwnRowShouldBeHighlighted()
    {
        var banner = await Leaderboard.GetBannerAsync();
        var all = await ReadWholeRankingAsync();
        all.Count(r => r.IsCurrentUser).Should().Be(1);
        all.Single(r => r.IsCurrentUser).Position.Should().Be(banner.Position);
    }

    // ----- pagination -----

    [Then("I should see {int} users on the page")]
    public async Task ThenIShouldSeeUsersOnThePage(int count)
    {
        (await Leaderboard.GetRowsAsync()).Should().HaveCount(count);
    }

    [Then("the page should list between {int} and {int} users")]
    public async Task ThenThePageShouldListBetween(int min, int max)
    {
        (await Leaderboard.GetRowsAsync()).Count.Should().BeInRange(min, max);
    }

    [Then("the first position on the page should be {int}")]
    public async Task ThenTheFirstPositionShouldBe(int position)
    {
        (await Leaderboard.GetRowsAsync()).First().Position.Should().Be(position);
    }

    [Then("the leaderboard should have more than one page")]
    public async Task ThenTheLeaderboardShouldHaveMoreThanOnePage()
    {
        (await Leaderboard.GetPageIndicatorAsync()).TotalPages.Should().BeGreaterThan(1);
    }

    [When("I go to the next page")]
    public async Task WhenIGoToTheNextPage() => await Leaderboard.ClickNextAsync();

    [When("I go to the previous page")]
    public async Task WhenIGoToThePreviousPage() => await Leaderboard.ClickPreviousAsync();

    [When("I go to the last page")]
    public async Task WhenIGoToTheLastPage()
    {
        while (!await Leaderboard.IsNextDisabledAsync())
        {
            await Leaderboard.ClickNextAsync();
        }
    }

    [Then("the previous page button should be disabled")]
    public async Task ThenPreviousShouldBeDisabled() => (await Leaderboard.IsPreviousDisabledAsync()).Should().BeTrue();

    [Then("the previous page button should be enabled")]
    public async Task ThenPreviousShouldBeEnabled() => (await Leaderboard.IsPreviousDisabledAsync()).Should().BeFalse();

    [Then("the next page button should be disabled")]
    public async Task ThenNextShouldBeDisabled() => (await Leaderboard.IsNextDisabledAsync()).Should().BeTrue();

    [Then("the next page button should be enabled")]
    public async Task ThenNextShouldBeEnabled() => (await Leaderboard.IsNextDisabledAsync()).Should().BeFalse();

    // ----- banner -----

    [Then("the banner should match the ranking table")]
    public async Task ThenTheBannerShouldMatchTheRankingTable()
    {
        var banner = await Leaderboard.GetBannerAsync();
        var ranking = await ReadWholeRankingAsync();

        var index = ranking.FindIndex(r => r.IsCurrentUser);
        index.Should().BeGreaterThanOrEqualTo(0, "the current user is part of the leaderboard");
        var me = ranking[index];

        banner.Position.Should().Be(me.Position);
        banner.Points.Should().Be(me.Points);

        if (index == 0)
        {
            banner.IsLeader.Should().BeTrue();
            banner.RivalText.Should().BeNull();
        }
        else
        {
            var rival = ranking[index - 1];
            banner.IsLeader.Should().BeFalse();
            banner.RivalText.Should().NotBeNull();
            banner.RivalText.Should().Contain(rival.User, "the rival is the user directly above");
            if (rival.Points == me.Points)
            {
                banner.RivalText.Should().Contain("same number of points");
            }
            else
            {
                var gap = rival.Points - me.Points;
                banner.RivalText.Should().Contain($"{gap} {(gap == 1 ? "point" : "points")} more than you");
            }
        }

        var expectedSame = ranking.Count(r => r.Points == me.Points) - 1;
        (banner.SamePointsCount ?? 0).Should().Be(expectedSame);
    }

    [Then("the banner should name a rival above me with the same points who reached them first")]
    public async Task ThenTheBannerShouldNameASamePointsRival()
    {
        var banner = await Leaderboard.GetBannerAsync();
        banner.IsLeader.Should().BeFalse();
        banner.RivalText.Should().Contain("same number of points").And.Contain("reached them first");
    }

    [Then("the banner should say I have at least {int} rivals with the same points")]
    public async Task ThenTheBannerShouldSayAtLeastRivals(int count)
    {
        (await Leaderboard.GetBannerAsync()).SamePointsCount.Should().BeGreaterThanOrEqualTo(count);
    }

    // ----- helpers -----

    /// <summary>Reads every page of the leaderboard from the first to the last and returns to the first page.</summary>
    private async Task<List<LeaderboardRowInfo>> ReadWholeRankingAsync()
    {
        while (!await Leaderboard.IsPreviousDisabledAsync())
        {
            await Leaderboard.ClickPreviousAsync();
        }

        var all = new List<LeaderboardRowInfo>(await Leaderboard.GetRowsAsync());
        while (!await Leaderboard.IsNextDisabledAsync())
        {
            await Leaderboard.ClickNextAsync();
            all.AddRange(await Leaderboard.GetRowsAsync());
        }

        while (!await Leaderboard.IsPreviousDisabledAsync())
        {
            await Leaderboard.ClickPreviousAsync();
        }
        return all;
    }
}
