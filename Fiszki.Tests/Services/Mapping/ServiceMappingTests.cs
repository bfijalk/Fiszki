using AutoFixture;
using FluentAssertions;
using Fiszki.Database.Entities;
using Fiszki.Services.Mapping;
using Xunit;

namespace Fiszki.Tests.Services.Mapping;

public class ServiceMappingTests
{
    private readonly Fixture _fixture;

    public ServiceMappingTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ToDto_WithUserEntity_ShouldMapCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.Basic,
            IsActive = true,
            TotalCardsGenerated = 10,
            TotalCardsAccepted = 8,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(user.Id);
        dto.Email.Should().Be(user.Email);
        dto.Role.Should().Be("Basic");
        dto.IsActive.Should().Be(user.IsActive);
        dto.TotalCardsGenerated.Should().Be(user.TotalCardsGenerated);
        dto.TotalCardsAccepted.Should().Be(user.TotalCardsAccepted);
        dto.CreatedAt.Should().Be(user.CreatedAt);
        dto.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    [Fact]
    public void ToDto_WithAdminUserEntity_ShouldMapRoleCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Role.Should().Be("Admin");
    }

    [Fact]
    public void ToDto_WithFlashcardEntity_ShouldMapCorrectly()
    {
        // Arrange
        var flashcard = new Flashcard
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FrontContent = "What is the capital of Poland?",
            BackContent = "Warsaw",
            CreationSource = CreationSource.Manual,
            AiModel = null,
            Tags = new List<string> { "geography", "poland", "capitals" },
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var dto = flashcard.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(flashcard.Id);
        dto.FrontContent.Should().Be(flashcard.FrontContent);
        dto.BackContent.Should().Be(flashcard.BackContent);
        dto.CreationSource.Should().Be("Manual");
        dto.AiModel.Should().BeNull();
        dto.Tags.Should().BeEquivalentTo(flashcard.Tags);
        dto.CreatedAt.Should().Be(flashcard.CreatedAt);
        dto.UpdatedAt.Should().Be(flashcard.UpdatedAt);
    }

    [Fact]
    public void ToDto_WithAiGeneratedFlashcard_ShouldMapCorrectly()
    {
        // Arrange
        var flashcard = new Flashcard
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FrontContent = "Jak się masz?",
            BackContent = "How are you?",
            CreationSource = CreationSource.Ai,
            AiModel = "gpt-4o-mini",
            Tags = new List<string> { "polish", "greetings" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = flashcard.ToDto();

        // Assert
        dto.CreationSource.Should().Be("Ai");
        dto.AiModel.Should().Be("gpt-4o-mini");
    }

    [Fact]
    public void ToDto_WithLearningProgressEntity_ShouldMapCorrectly()
    {
        // Arrange
        var progress = new LearningProgress
        {
            Id = Guid.NewGuid(),
            FlashcardId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            EaseFactor = 2.5,
            Interval = 1,
            Repetitions = 0,
            NextReviewDate = DateTime.UtcNow.AddDays(1),
            LastReviewDate = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = progress.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.FlashcardId.Should().Be(progress.FlashcardId);
        dto.EaseFactor.Should().Be(progress.EaseFactor);
        dto.Interval.Should().Be(progress.Interval);
        dto.Repetitions.Should().Be(progress.Repetitions);
        dto.NextReviewDate.Should().Be(progress.NextReviewDate);
        dto.LastReviewDate.Should().BeNull();
    }

    [Fact]
    public void ToDto_WithReviewedLearningProgress_ShouldMapCorrectly()
    {
        // Arrange
        var lastReview = DateTime.UtcNow.AddDays(-2);
        var progress = new LearningProgress
        {
            Id = Guid.NewGuid(),
            FlashcardId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            EaseFactor = 2.8,
            Interval = 3,
            Repetitions = 2,
            NextReviewDate = DateTime.UtcNow.AddDays(3),
            LastReviewDate = lastReview,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Act
        var dto = progress.ToDto();

        // Assert
        dto.LastReviewDate.Should().Be(lastReview);
        dto.EaseFactor.Should().Be(2.8);
        dto.Interval.Should().Be(3);
        dto.Repetitions.Should().Be(2);
    }

    [Fact]
    public void ToDto_WithEmptyTagsList_ShouldMapCorrectly()
    {
        // Arrange
        var flashcard = new Flashcard
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FrontContent = "Front",
            BackContent = "Back",
            CreationSource = CreationSource.Manual,
            Tags = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = flashcard.ToDto();

        // Assert
        dto.Tags.Should().BeEmpty();
        dto.Tags.Should().NotBeNull();
    }
}
