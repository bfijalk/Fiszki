using FluentAssertions;
using FluentValidation;
using Fiszki.Database.Entities;
using Fiszki.Services.Commands;
using Fiszki.Services.Models;
using Fiszki.Services.Services;
using Fiszki.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Fiszki.Tests.Services;

public class FlashcardArchivingTests : IDisposable
{
    private readonly Database.FiszkiDbContext _db;
    private readonly FlashcardService _svc;

    public FlashcardArchivingTests()
    {
        _db = TestDbContextFactory.CreateInMemoryContext();
        _svc = new FlashcardService(
            _db,
            new Mock<IValidator<CreateFlashcardCommand>>().Object,
            new Mock<IValidator<UpdateFlashcardCommand>>().Object);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Flashcard> AddCardAsync(Guid userId, CreationSource source = CreationSource.Manual,
        DateTime? createdAt = null, DateTime? archivedAt = null)
    {
        var card = new Flashcard
        {
            Id = Guid.NewGuid(), UserId = userId, FrontContent = "Q", BackContent = "A",
            CreationSource = source, CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow, ArchivedAt = archivedAt
        };
        _db.Flashcards.Add(card);
        await _db.SaveChangesAsync();
        return card;
    }

    [Fact]
    public async Task ArchiveAsync_SetsArchivedAt()
    {
        var userId = Guid.NewGuid();
        var card = await AddCardAsync(userId);

        await _svc.ArchiveAsync(userId, card.Id);

        var stored = await _db.Flashcards.AsNoTracking().SingleAsync(f => f.Id == card.Id);
        stored.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ArchiveAsync_OtherUsersCard_ChangesNothing()
    {
        var card = await AddCardAsync(Guid.NewGuid());

        await _svc.ArchiveAsync(Guid.NewGuid(), card.Id);

        (await _db.Flashcards.AsNoTracking().SingleAsync(f => f.Id == card.Id)).ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task ArchiveAsync_MissingCard_DoesNotThrow()
    {
        var act = () => _svc.ArchiveAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ArchiveAsync_Repeated_KeepsFirstArchivedAt()
    {
        var userId = Guid.NewGuid();
        var first = DateTime.UtcNow.AddDays(-2);
        var card = await AddCardAsync(userId, archivedAt: first);

        await _svc.ArchiveAsync(userId, card.Id);

        (await _db.Flashcards.AsNoTracking().SingleAsync(f => f.Id == card.Id)).ArchivedAt.Should().Be(first);
    }

    [Fact]
    public async Task RestoreAsync_ClearsArchivedAt()
    {
        var userId = Guid.NewGuid();
        var card = await AddCardAsync(userId, archivedAt: DateTime.UtcNow);

        await _svc.RestoreAsync(userId, card.Id);

        (await _db.Flashcards.AsNoTracking().SingleAsync(f => f.Id == card.Id)).ArchivedAt.Should().BeNull();
    }

    [Fact]
    public async Task RestoreAsync_OtherUsersCard_ChangesNothing()
    {
        var card = await AddCardAsync(Guid.NewGuid(), archivedAt: DateTime.UtcNow);

        await _svc.RestoreAsync(Guid.NewGuid(), card.Id);

        (await _db.Flashcards.AsNoTracking().SingleAsync(f => f.Id == card.Id)).ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RestoreAsync_NotArchivedOrMissing_DoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var card = await AddCardAsync(userId);

        var act = async () =>
        {
            await _svc.RestoreAsync(userId, card.Id);
            await _svc.RestoreAsync(userId, Guid.NewGuid());
        };

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListAsync_ActiveByDefault_ExcludesArchived()
    {
        var userId = Guid.NewGuid();
        var active = await AddCardAsync(userId);
        await AddCardAsync(userId, archivedAt: DateTime.UtcNow);

        var list = await _svc.ListAsync(userId);

        list.Select(f => f.Id).Should().BeEquivalentTo(new[] { active.Id });
    }

    [Fact]
    public async Task ListAsync_Archived_ReturnsOnlyArchivedNewestArchivedFirst()
    {
        var userId = Guid.NewGuid();
        await AddCardAsync(userId);
        var older = await AddCardAsync(userId, archivedAt: DateTime.UtcNow.AddHours(-2));
        var newer = await AddCardAsync(userId, archivedAt: DateTime.UtcNow.AddHours(-1));

        var list = await _svc.ListAsync(userId, state: FlashcardListState.Archived);

        list.Select(f => f.Id).Should().Equal(newer.Id, older.Id);
        list.Should().OnlyContain(f => f.ArchivedAt != null);
    }

    [Fact]
    public async Task GetSummaryAsync_CountsArchivedInEveryTotal()
    {
        var userId = Guid.NewGuid();
        await AddCardAsync(userId, CreationSource.Ai, archivedAt: DateTime.UtcNow);
        await AddCardAsync(userId, CreationSource.Ai);
        await AddCardAsync(userId, CreationSource.Manual);
        await AddCardAsync(Guid.NewGuid(), CreationSource.Manual, archivedAt: DateTime.UtcNow); // another user

        var summary = await _svc.GetSummaryAsync(userId);

        summary.Total.Should().Be(3);
        summary.Ai.Should().Be(2);
        summary.Manual.Should().Be(1);
        summary.Archived.Should().Be(1);
        (summary.Ai + summary.Manual).Should().Be(summary.Total);
        summary.LearningProgress.Should().Be(33);
    }

    [Fact]
    public async Task GetSummaryAsync_NoFlashcards_IsZero()
    {
        var summary = await _svc.GetSummaryAsync(Guid.NewGuid());

        summary.Should().Be(new FlashcardSummaryDto(0, 0, 0, 0, 0));
    }

    [Fact]
    public async Task ArchiveThenRestore_MovesProgressUpAndDown()
    {
        var userId = Guid.NewGuid();
        var cards = new List<Flashcard>();
        for (var i = 0; i < 4; i++) cards.Add(await AddCardAsync(userId));

        await _svc.ArchiveAsync(userId, cards[0].Id);
        (await _svc.GetSummaryAsync(userId)).LearningProgress.Should().Be(25);
        await _svc.ArchiveAsync(userId, cards[1].Id);
        (await _svc.GetSummaryAsync(userId)).LearningProgress.Should().Be(50);
        await _svc.RestoreAsync(userId, cards[1].Id);
        (await _svc.GetSummaryAsync(userId)).LearningProgress.Should().Be(25);
    }

    [Fact]
    public async Task DeleteAsync_ArchivedCard_Removes_AndChangesTheBase()
    {
        var userId = Guid.NewGuid();
        await AddCardAsync(userId);
        await AddCardAsync(userId);
        var archived1 = await AddCardAsync(userId, archivedAt: DateTime.UtcNow);
        await AddCardAsync(userId, archivedAt: DateTime.UtcNow);
        (await _svc.GetSummaryAsync(userId)).LearningProgress.Should().Be(50);

        await _svc.DeleteAsync(userId, archived1.Id);

        (await _db.Flashcards.AnyAsync(f => f.Id == archived1.Id)).Should().BeFalse();
        var summary = await _svc.GetSummaryAsync(userId);
        summary.Total.Should().Be(3);
        summary.Archived.Should().Be(1);
        summary.LearningProgress.Should().Be(33);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 5, 0)]
    [InlineData(1, 3, 33)]
    [InlineData(2, 3, 66)]
    [InlineData(1, 4, 25)]
    [InlineData(199, 200, 99)]
    [InlineData(3, 3, 100)]
    public void LearningProgressCalculator_RoundsDown_AndReaches100OnlyWhenAllArchived(int archived, int total, int expected)
    {
        LearningProgressCalculator.Percent(archived, total).Should().Be(expected);
    }
}
