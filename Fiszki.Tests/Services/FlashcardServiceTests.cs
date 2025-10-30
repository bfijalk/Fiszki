using AutoFixture.Xunit2;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Fiszki.Database.Entities;
using Fiszki.Services.Commands;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Services;
using Fiszki.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Fiszki.Tests.Services;

public class FlashcardServiceTests : IDisposable
{
    private readonly Database.FiszkiDbContext _db;
    private readonly Mock<IValidator<CreateFlashcardCommand>> _createValidator;
    private readonly Mock<IValidator<UpdateFlashcardCommand>> _updateValidator;
    private readonly FlashcardService _svc;

    public FlashcardServiceTests()
    {
        _db = TestDbContextFactory.CreateInMemoryContext();
        _createValidator = new Mock<IValidator<CreateFlashcardCommand>>();
        _updateValidator = new Mock<IValidator<UpdateFlashcardCommand>>();
        _svc = new FlashcardService(_db, _createValidator.Object, _updateValidator.Object);
    }

    public void Dispose() => _db.Dispose();

    [Theory]
    [AutoData]
    public async Task CreateAsync_ShouldPersistAndReturnDto(Guid userId, string front, string back)
    {
        var cmd = new CreateFlashcardCommand(front, back, new [] { "Tag1", " tag1 ", "Tag2" });
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var dto = await _svc.CreateAsync(userId, cmd);

        dto.Id.Should().NotBeEmpty();
        dto.FrontContent.Should().Be(front.Trim());
        dto.BackContent.Should().Be(back.Trim());
        dto.CreationSource.Should().Be("Manual");
        dto.Tags.Should().BeEquivalentTo(new [] { "Tag1", "Tag2" });

        var entity = await _db.Flashcards.SingleAsync(f => f.Id == dto.Id);
        entity.UserId.Should().Be(userId);
    }

    [Theory]
    [AutoData]
    public async Task CreateAsync_Invalid_ShouldThrow(Guid userId, string front, string back)
    {
        var cmd = new CreateFlashcardCommand(front, back, null);
        _createValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateFlashcardCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new [] { new ValidationFailure("FrontContent", "fail") }));

        var act = () => _svc.CreateAsync(userId, cmd);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [AutoData]
    public async Task GetAsync_ReturnsNullForWrongUser(Guid ownerId, Guid otherUserId, string front, string back)
    {
        var card = new Flashcard
        {
            Id = Guid.NewGuid(), UserId = ownerId, FrontContent = front, BackContent = back,
            CreationSource = CreationSource.Manual, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Flashcards.Add(card);
        await _db.SaveChangesAsync();

        var dto = await _svc.GetAsync(otherUserId, card.Id);
        dto.Should().BeNull();
    }

    [Theory]
    [AutoData]
    public async Task ListAsync_ReturnsOrdered(Guid userId)
    {
        for (int i = 0; i < 3; i++)
        {
            _db.Flashcards.Add(new Flashcard
            {
                Id = Guid.NewGuid(), UserId = userId, FrontContent = $"F{i}", BackContent = $"B{i}",
                CreationSource = CreationSource.Manual, CreatedAt = DateTime.UtcNow.AddMinutes(i), UpdatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await _db.SaveChangesAsync();
        var list = await _svc.ListAsync(userId);
        list.Should().HaveCount(3);
        list.Select(x => x.CreatedAt).Should().BeInDescendingOrder();
    }

    [Theory]
    [AutoData]
    public async Task UpdateAsync_ConcurrencyConflict_ShouldThrow(Guid userId, string front, string back)
    {
        var card = new Flashcard
        {
            Id = Guid.NewGuid(), UserId = userId, FrontContent = front, BackContent = back,
            CreationSource = CreationSource.Manual, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Flashcards.Add(card);
        await _db.SaveChangesAsync();

        var staleTimestamp = card.UpdatedAt.AddSeconds(-30);
        var cmd = new UpdateFlashcardCommand(card.Id, front + "x", back + "y", null, staleTimestamp);
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var act = () => _svc.UpdateAsync(userId, cmd);
        await act.Should().ThrowAsync<ConflictException>().WithMessage("Flashcard has been modified by another operation");
    }

    [Theory]
    [AutoData]
    public async Task UpdateAsync_NotFound_ShouldThrow(Guid userId, Guid cardId, string front, string back)
    {
        var cmd = new UpdateFlashcardCommand(cardId, front, back, null, null);
        _updateValidator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var act = () => _svc.UpdateAsync(userId, cmd);
        await act.Should().ThrowAsync<DomainNotFoundException>().WithMessage("Flashcard not found");
    }

    [Theory]
    [AutoData]
    public async Task DeleteAsync_ShouldRemove(Guid userId, string front, string back)
    {
        var card = new Flashcard
        {
            Id = Guid.NewGuid(), UserId = userId, FrontContent = front, BackContent = back,
            CreationSource = CreationSource.Manual, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Flashcards.Add(card);
        await _db.SaveChangesAsync();

        await _svc.DeleteAsync(userId, card.Id);
        (await _db.Flashcards.FindAsync(card.Id)).Should().BeNull();
    }

    [Theory]
    [AutoData]
    public async Task DeleteAsync_Idempotent(Guid userId, Guid cardId)
    {
        var act = () => _svc.DeleteAsync(userId, cardId);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [AutoData]
    public async Task DeleteAsync_ShouldNotDeleteOtherUsersCard(Guid ownerId, Guid otherUserId, string front, string back)
    {
        // Arrange: Create a flashcard owned by one user
        var card = new Flashcard
        {
            Id = Guid.NewGuid(), 
            UserId = ownerId, 
            FrontContent = front, 
            BackContent = back,
            CreationSource = CreationSource.Manual, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow
        };
        _db.Flashcards.Add(card);
        await _db.SaveChangesAsync();

        // Act: Try to delete the card using a different user ID
        await _svc.DeleteAsync(otherUserId, card.Id);

        // Assert: The card should still exist in the database (not deleted)
        var existingCard = await _db.Flashcards.FindAsync(card.Id);
        existingCard.Should().NotBeNull("other users should not be able to delete cards they don't own");
        existingCard!.UserId.Should().Be(ownerId, "the card should still belong to the original owner");
    }

    [Theory]
    [AutoData]
    public async Task DeleteAsync_ShouldOnlyAffectSpecifiedCard(Guid userId, string front1, string back1, string front2, string back2)
    {
        // Arrange: Create multiple flashcards for the same user
        var card1 = new Flashcard
        {
            Id = Guid.NewGuid(), 
            UserId = userId, 
            FrontContent = front1, 
            BackContent = back1,
            CreationSource = CreationSource.Manual, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow
        };
        var card2 = new Flashcard
        {
            Id = Guid.NewGuid(), 
            UserId = userId, 
            FrontContent = front2, 
            BackContent = back2,
            CreationSource = CreationSource.Ai, 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow
        };
        _db.Flashcards.AddRange(card1, card2);
        await _db.SaveChangesAsync();

        // Act: Delete only the first card
        await _svc.DeleteAsync(userId, card1.Id);

        // Assert: Only the first card should be deleted, the second should remain
        (await _db.Flashcards.FindAsync(card1.Id)).Should().BeNull("the specified card should be deleted");
        (await _db.Flashcards.FindAsync(card2.Id)).Should().NotBeNull("other cards should not be affected");
        
        var remainingCard = await _db.Flashcards.FindAsync(card2.Id);
        remainingCard!.FrontContent.Should().Be(front2);
        remainingCard.BackContent.Should().Be(back2);
    }
}
