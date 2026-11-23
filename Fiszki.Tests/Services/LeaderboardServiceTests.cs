using FluentAssertions;
using Fiszki.Database;
using Fiszki.Database.Entities;
using Fiszki.Services;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Services;
using Fiszki.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fiszki.Tests.Services;

public class LeaderboardServiceTests : IDisposable
{
    private static readonly DateTime Start = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FiszkiDbContext _db;
    private readonly LeaderboardService _svc;
    private int _userCounter;

    public LeaderboardServiceTests()
    {
        _db = TestDbContextFactory.CreateInMemoryContext();
        _svc = new LeaderboardService(_db);
    }

    public void Dispose() => _db.Dispose();

    private User AddUser(string? email = null, bool active = true)
    {
        _userCounter++;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email ?? $"user{_userCounter:D3}@example.com",
            PasswordHash = "hash",
            IsActive = active,
            CreatedAt = Start.AddMinutes(_userCounter),
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user;
    }

    private List<Flashcard> AddCards(User user, int count, DateTime? firstAt = null, CreationSource source = CreationSource.Manual)
    {
        var first = firstAt ?? Start.AddDays(1);
        var cards = new List<Flashcard>();
        for (var i = 0; i < count; i++)
        {
            cards.Add(new Flashcard
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FrontContent = "front",
                BackContent = "back",
                CreationSource = source,
                CreatedAt = first.AddMinutes(i),
            });
        }
        _db.Flashcards.AddRange(cards);
        _db.SaveChanges();
        return cards;
    }

    [Fact]
    public async Task HigherPointsRankHigher()
    {
        var low = AddUser();
        var high = AddUser();
        AddCards(low, 7);
        AddCards(high, 10);

        var result = await _svc.GetAsync(low.Id);

        result.Rows.Select(r => r.Points).Should().Equal(10, 7);
        result.Rows.Select(r => r.Position).Should().Equal(1, 2);
    }

    [Fact]
    public async Task AiAndManualFlashcardsCountEqually()
    {
        var user = AddUser();
        AddCards(user, 3, source: CreationSource.Ai);
        AddCards(user, 2, firstAt: Start.AddDays(2), source: CreationSource.Manual);

        var result = await _svc.GetAsync(user.Id);

        result.Rows.Single().Points.Should().Be(5);
    }

    [Fact]
    public async Task Tie_IsBrokenByEarlierLastCard_AndPositionsAreUnique()
    {
        var later = AddUser();
        var earlier = AddUser();
        AddCards(later, 7, firstAt: Start.AddDays(5));
        AddCards(earlier, 7, firstAt: Start.AddDays(1));

        var result = await _svc.GetAsync(later.Id);

        result.Rows.Select(r => r.IsCurrentUser).Should().Equal(false, true);
        result.Rows.Select(r => r.Position).Should().Equal(1, 2);
    }

    [Fact]
    public async Task ZeroPointUsers_AreListedLast_WithUniquePositions()
    {
        var zeroA = AddUser();
        var zeroB = AddUser();
        var withCards = AddUser();
        AddCards(withCards, 1);

        var result = await _svc.GetAsync(zeroA.Id);

        result.Rows.Select(r => r.Points).Should().Equal(1, 0, 0);
        result.Rows.Select(r => r.Position).Should().Equal(1, 2, 3);
        result.TotalUsers.Should().Be(3);
        _ = zeroB;
    }

    [Fact]
    public async Task InactiveUsers_AreNotListedAndDoNotTakePositions()
    {
        var current = AddUser();
        var inactive = AddUser(active: false);
        AddCards(current, 1);
        AddCards(inactive, 20);

        var result = await _svc.GetAsync(current.Id);

        result.Rows.Should().ContainSingle();
        result.Rows.Single().Position.Should().Be(1);
        result.TotalUsers.Should().Be(1);
    }

    [Fact]
    public async Task InactiveCurrentUser_Throws()
    {
        var inactive = AddUser(active: false);

        var act = () => _svc.GetAsync(inactive.Id);

        await act.Should().ThrowAsync<DomainNotFoundException>();
    }

    [Fact]
    public async Task DeletingFlashcard_LowersPoints()
    {
        var user = AddUser();
        var cards = AddCards(user, 5);
        (await _svc.GetAsync(user.Id)).Banner.Points.Should().Be(5);

        _db.Flashcards.Remove(cards[0]);
        _db.SaveChanges();

        (await _svc.GetAsync(user.Id)).Banner.Points.Should().Be(4);
    }

    [Fact]
    public async Task Paging_FirstAndLastPageSizes_AndClamping()
    {
        User? last = null;
        for (var i = 0; i < 70; i++)
        {
            last = AddUser();
            AddCards(last, 70 - i);
        }

        var first = await _svc.GetAsync(last!.Id, 1);
        var third = await _svc.GetAsync(last.Id, 3);
        var beyond = await _svc.GetAsync(last.Id, 99);
        var below = await _svc.GetAsync(last.Id, 0);

        first.Rows.Should().HaveCount(30);
        first.Rows.First().Position.Should().Be(1);
        first.Rows.Last().Position.Should().Be(30);
        first.TotalPages.Should().Be(3);
        third.Rows.Should().HaveCount(10);
        third.Rows.First().Position.Should().Be(61);
        third.Rows.Last().Position.Should().Be(70);
        beyond.Page.Should().Be(3);
        below.Page.Should().Be(1);
    }

    [Fact]
    public async Task Banner_RivalAhead_ShowsGapAndRivalMaskedEmail()
    {
        var rival = AddUser("rival.person@example.com");
        var me = AddUser();
        AddCards(rival, 14);
        AddCards(me, 12);

        var banner = (await _svc.GetAsync(me.Id)).Banner;

        banner.Position.Should().Be(2);
        banner.IsLeader.Should().BeFalse();
        banner.RivalMaskedEmail.Should().Be("ri***@example.com");
        banner.PointsGap.Should().Be(2);
        banner.SamePointsCount.Should().Be(0);
    }

    [Fact]
    public async Task Banner_Leader_HasNoRival()
    {
        var me = AddUser();
        var other = AddUser();
        AddCards(me, 5);
        AddCards(other, 2);

        var banner = (await _svc.GetAsync(me.Id)).Banner;

        banner.IsLeader.Should().BeTrue();
        banner.Position.Should().Be(1);
        banner.RivalMaskedEmail.Should().BeNull();
        banner.PointsGap.Should().BeNull();
    }

    [Fact]
    public async Task Banner_SamePointsCount_CountsOtherUsersWithEqualPoints()
    {
        var me = AddUser();
        var a = AddUser();
        var b = AddUser();
        AddCards(me, 12, firstAt: Start.AddDays(9));
        AddCards(a, 12, firstAt: Start.AddDays(1));
        AddCards(b, 12, firstAt: Start.AddDays(2));

        var banner = (await _svc.GetAsync(me.Id)).Banner;

        banner.SamePointsCount.Should().Be(2);
        banner.Position.Should().Be(3);
        banner.PointsGap.Should().Be(0);
    }

    [Fact]
    public async Task Result_NeverContainsFullEmail()
    {
        var me = AddUser("jan.kowalski@example.com");
        AddCards(me, 1);

        var result = await _svc.GetAsync(me.Id);

        result.Rows.Single().MaskedEmail.Should().Be("ja***@example.com");
        result.Rows.Select(r => r.MaskedEmail).Should().NotContain(e => e.Contains("kowalski"));
    }

    [Fact]
    public void Service_IsRegisteredInDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_db);
        services.AddFiszkiServices(new ConfigurationBuilder().Build());

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<ILeaderboardService>().Should().BeOfType<LeaderboardService>();
    }
}
