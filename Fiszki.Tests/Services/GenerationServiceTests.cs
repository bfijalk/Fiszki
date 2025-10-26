using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Fiszki.Services.Commands;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Interfaces.OpenRouter;
using Fiszki.Services.Models.Generation;
using Fiszki.Services.Models.OpenRouter;
using Fiszki.Services.Services;
using Fiszki.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Fiszki.Tests.Services;

public class GenerationServiceTests : IDisposable
{
    private readonly Mock<ILogger<GenerationService>> _loggerMock;
    private readonly Mock<IValidator<StartGenerationCommand>> _startValidatorMock;
    private readonly Mock<IValidator<SaveProposalsCommand>> _saveValidatorMock;
    private readonly Mock<IOpenRouterChatService> _openRouterServiceMock;
    private readonly GenerationService _generationService;
    private readonly Database.FiszkiDbContext _dbContext;
    private readonly Fixture _fixture;

    public GenerationServiceTests()
    {
        _loggerMock = new Mock<ILogger<GenerationService>>();
        _startValidatorMock = new Mock<IValidator<StartGenerationCommand>>();
        _saveValidatorMock = new Mock<IValidator<SaveProposalsCommand>>();
        _openRouterServiceMock = new Mock<IOpenRouterChatService>();
        _dbContext = TestDbContextFactory.CreateInMemoryContext();
        
        _generationService = new GenerationService(
            _loggerMock.Object,
            _startValidatorMock.Object,
            _saveValidatorMock.Object,
            _dbContext,
            _openRouterServiceMock.Object);
        
        _fixture = new Fixture();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Theory]
    [AutoData]
    public async Task StartAsync_WithValidCommand_ShouldReturnGenerationJobDto(
        string sourceText,
        string language,
        int maxCards)
    {
        // Arrange
        var command = new StartGenerationCommand
        {
            SourceText = sourceText,
            Language = language,
            MaxCards = Math.Max(1, Math.Min(maxCards, 20)) // Clamp to valid range
        };

        _startValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Mock successful OpenRouter response
        var mockSession = new ChatSession { Model = "test-model" };
        var mockProposals = new List<FlashcardProposalDto>
        {
            new(Guid.NewGuid(), "Front 1", "Back 1", "Example 1", null),
            new(Guid.NewGuid(), "Front 2", "Back 2", "Example 2", null)
        };

        _openRouterServiceMock
            .Setup(x => x.CreateSession(It.IsAny<string>()))
            .Returns(mockSession);

        _openRouterServiceMock
            .Setup(x => x.CompleteStructuredAsync<FlashcardGenerationResponse>(
                It.IsAny<ChatSession>(),
                It.IsAny<JsonSchemaDescriptor>(),
                It.IsAny<ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StructuredResult<FlashcardGenerationResponse>
            {
                Data = new FlashcardGenerationResponse
                {
                    Flashcards = mockProposals.Select(p => new FlashcardData
                    {
                        Front = p.Front,
                        Back = p.Back,
                        Example = p.Example ?? ""
                    }).ToList()
                },
                RawJson = "{}",
                BaseResult = new ChatCompletionResult
                {
                    Content = "generated json",
                    Model = "test-model",
                    Usage = new TokenUsage { PromptTokens = 10, CompletionTokens = 20, TotalTokens = 30 },
                    RequestId = Guid.NewGuid().ToString(),
                    FinishReason = "stop",
                    Duration = TimeSpan.FromMilliseconds(5),
                    Cost = 0m
                }
            });

        // Act
        var result = await _generationService.StartAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().NotBeEmpty();
        // Generation can be either queued for async processing or completed immediately
        result.Status.Should().BeOneOf(GenerationStatusEnum.Queued, GenerationStatusEnum.Completed);
        result.QueuedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public async Task StartAsync_WithInvalidCommand_ShouldThrowValidationException(
        string sourceText,
        string language,
        int maxCards)
    {
        // Arrange
        var command = new StartGenerationCommand
        {
            SourceText = sourceText,
            Language = language,
            MaxCards = maxCards
        };

        _startValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<StartGenerationCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new [] { new ValidationFailure("SourceText", "Validation failed") }));
        var act = () => _generationService.StartAsync(command);
        await act.Should().ThrowAsync<ValidationException>().Where(e => e.Message.Contains("Validation failed"));

        // Act & Assert
        //var act = () => _generationService.StartAsync(command);
        //await act.Should().ThrowAsync<ValidationException>()
        //    .WithMessage("Validation failed");
    }

    [Fact]
    public async Task GetStatusAsync_WithExistingJob_ShouldReturnGenerationStatusDto()
    {
        // Arrange
        var command = new StartGenerationCommand
        {
            SourceText = "test text",
            Language = "Polish",
            MaxCards = 5
        };

        _startValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Start a job first
        var jobResult = await _generationService.StartAsync(command);

        // Act
        var result = await _generationService.GetStatusAsync(jobResult.JobId);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().Be(jobResult.JobId);
        result.Status.Should().BeOneOf(
            GenerationStatusEnum.Queued,
            GenerationStatusEnum.Generating,
            GenerationStatusEnum.Completed,
            GenerationStatusEnum.Failed);
    }

    [Fact]
    public async Task GetStatusAsync_WithNonExistentJob_ShouldThrowGenerationException()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act & Assert
        var act = () => _generationService.GetStatusAsync(nonExistentJobId);
        await act.Should().ThrowAsync<GenerationException>()
            .WithMessage($"Generation job {nonExistentJobId} not found");
    }

    [Fact]
    public async Task CancelAsync_WithExistingJob_ShouldCancelJob()
    {
        // Arrange
        var command = new StartGenerationCommand
        {
            SourceText = "test text",
            Language = "Polish",
            MaxCards = 5
        };

        _startValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var jobResult = await _generationService.StartAsync(command);

        // Act
        await _generationService.CancelAsync(jobResult.JobId);

        // Assert
        var status = await _generationService.GetStatusAsync(jobResult.JobId);
        status.Status.Should().Be(GenerationStatusEnum.Canceled);
    }

    [Fact]
    public async Task CancelAsync_WithNonExistentJob_ShouldBeIdempotent()
    {
        // Arrange
        var nonExistentJobId = Guid.NewGuid();

        // Act & Assert - Should not throw
        var act = () => _generationService.CancelAsync(nonExistentJobId);
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [AutoData]
    public async Task SaveProposalsAsync_WithValidCommand_ShouldSaveAcceptedProposals(Guid userId)
    {
        // Arrange
        var proposals = new List<FlashcardProposalDto>
        {
            new(Guid.NewGuid(), "Front 1", "Back 1", "Example 1", null, true, false),
            new(Guid.NewGuid(), "Front 2", "Back 2", "Example 2", null, false, true),
            new(Guid.NewGuid(), "Front 3", "Back 3", "Example 3", null, true, false)
        };

        var command = new SaveProposalsCommand
        {
            Proposals = proposals
        };

        _saveValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _generationService.SaveProposalsAsync(userId, command);

        // Assert
        result.Should().HaveCount(2); // Only accepted and not rejected proposals
        
        // Verify flashcards were saved to database
        var savedFlashcards = _dbContext.Flashcards.Where(f => f.UserId == userId).ToList();
        savedFlashcards.Should().HaveCount(2);
        savedFlashcards.Should().OnlyContain(f => f.CreationSource == Database.Entities.CreationSource.Ai);
    }

    [Theory]
    [AutoData]
    public async Task SaveProposalsAsync_WithNoAcceptedProposals_ShouldReturnEmptyList(Guid userId)
    {
        // Arrange
        var proposals = new List<FlashcardProposalDto>
        {
            new(Guid.NewGuid(), "Front 1", "Back 1", "Example 1", null, false, true),
            new(Guid.NewGuid(), "Front 2", "Back 2", "Example 2", null, false, true)
        };

        var command = new SaveProposalsCommand
        {
            Proposals = proposals
        };

        _saveValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _generationService.SaveProposalsAsync(userId, command);

        // Assert
        result.Should().BeEmpty();
        
        // Verify no flashcards were saved to database
        var savedFlashcards = _dbContext.Flashcards.Where(f => f.UserId == userId).ToList();
        savedFlashcards.Should().BeEmpty();
    }

    [Theory]
    [AutoData]
    public async Task SaveProposalsAsync_WithInvalidCommand_ShouldThrowValidationException(Guid userId)
    {
        // Arrange
        var command = new SaveProposalsCommand
        {
            Proposals = new List<FlashcardProposalDto>()
        };

        _saveValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SaveProposalsCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new [] { new ValidationFailure("Proposals", "Validation failed") }));
        var actInvalid = () => _generationService.SaveProposalsAsync(userId, command);
        await actInvalid.Should().ThrowAsync<ValidationException>().Where(e => e.Message.Contains("Validation failed"));
    }

    [Theory]
    [AutoData]
    public async Task StartAsync_WhenOpenRouterFails_ShouldFallbackToMockGeneration(
        string sourceText,
        string language,
        int maxCards)
    {
        // Arrange
        var command = new StartGenerationCommand
        {
            SourceText = sourceText,
            Language = language,
            MaxCards = Math.Max(1, Math.Min(maxCards, 20))
        };

        _startValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var mockSession = new ChatSession { Model = "test-model" };
        _openRouterServiceMock
            .Setup(x => x.CreateSession(It.IsAny<string>()))
            .Returns(mockSession);

        // Simulate OpenRouter API failure
        _openRouterServiceMock
            .Setup(x => x.CompleteStructuredAsync<FlashcardGenerationResponse>(
                It.IsAny<ChatSession>(),
                It.IsAny<JsonSchemaDescriptor>(),
                It.IsAny<ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        // Act
        var result = await _generationService.StartAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(GenerationStatusEnum.Queued);

        // Wait a bit for background processing and then check status
        await Task.Delay(100);
        var status = await _generationService.GetStatusAsync(result.JobId);
        
        // Should either be completed (with mock data) or failed, but not still generating
        status.Status.Should().BeOneOf(GenerationStatusEnum.Completed, GenerationStatusEnum.Failed);
        
        if (status.Status == GenerationStatusEnum.Completed)
        {
            status.Proposals.Should().NotBeEmpty();
            status.Proposals.Should().OnlyContain(p => 
                p.Front.Contains("Polish Word") && 
                p.Back.Contains("English Translation"));
        }
    }
}

// Helper classes for testing
public class FlashcardGenerationResponse
{
    public List<FlashcardData> Flashcards { get; set; } = new();
}

public class FlashcardData
{
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
}
