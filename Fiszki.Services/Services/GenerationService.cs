using Fiszki.Services.Commands;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Interfaces.OpenRouter;
using Fiszki.Services.Models.Generation;
using Fiszki.Services.Models.OpenRouter;
using Fiszki.Services.Exceptions;
using Fiszki.Services.Mapping;
using Fiszki.Database;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Fiszki.Services.Services;

public class GenerationService : IGenerationService
{
    private readonly ILogger<GenerationService> _logger;
    private readonly IValidator<StartGenerationCommand> _startValidator;
    private readonly IValidator<SaveProposalsCommand> _saveValidator;
    private readonly FiszkiDbContext _dbContext;
    private readonly IOpenRouterChatService _openRouterService;
    private readonly ConcurrentDictionary<Guid, GenerationJob> _jobs = new();

    public GenerationService(
        ILogger<GenerationService> logger,
        IValidator<StartGenerationCommand> startValidator,
        IValidator<SaveProposalsCommand> saveValidator,
        FiszkiDbContext dbContext,
        IOpenRouterChatService openRouterService)
    {
        _logger = logger;
        _startValidator = startValidator;
        _saveValidator = saveValidator;
        _dbContext = dbContext;
        _openRouterService = openRouterService;
    }

    public async Task<GenerationJobDto> StartAsync(StartGenerationCommand command, CancellationToken ct = default)
    {
        var vr = await _startValidator.ValidateAsync(new ValidationContext<StartGenerationCommand>(command), ct);
        if (!vr.IsValid)
        {
            throw new ValidationException("Validation failed", vr.Errors);
        }

        var job = new GenerationJob
        {
            Id = Guid.NewGuid(),
            QueuedAt = DateTime.UtcNow,
            Status = GenerationStatusEnum.Queued,
            Command = command
        };

        _jobs.TryAdd(job.Id, job);
        
        // Generate flashcards using OpenRouter in background
        _ = Task.Run(async () =>
        {
            try
            {
                job.Status = GenerationStatusEnum.Generating;
                _logger.LogInformation("Starting flashcard generation for job {JobId} with topic: {Topic}", 
                    job.Id, command.SourceText);
                
                job.Proposals = await GenerateFlashcardsWithOpenRouter(command, ct);
                job.Status = GenerationStatusEnum.Completed;
                
                _logger.LogInformation("Completed flashcard generation for job {JobId}. Generated {Count} proposals", 
                    job.Id, job.Proposals.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate flashcards for job {JobId}", job.Id);
                job.Status = GenerationStatusEnum.Failed;
                job.Error = "Failed to generate flashcards. Please try again.";
            }
        });

        return new GenerationJobDto(job.Id, job.QueuedAt, job.Status);
    }

    public Task<GenerationStatusDto> GetStatusAsync(Guid jobId, CancellationToken ct = default)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            throw new GenerationException($"Generation job {jobId} not found", GenerationStatusEnum.Failed);
        }

        return Task.FromResult(new GenerationStatusDto(
            job.Id,
            job.Status,
            job.Proposals?.Count ?? 0,
            job.Proposals,
            job.Error
        ));
    }

    public Task CancelAsync(Guid jobId, CancellationToken ct = default)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = GenerationStatusEnum.Canceled;
        }

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Guid>> SaveProposalsAsync(Guid userId, SaveProposalsCommand command, CancellationToken ct = default)
    {
        var vr = await _saveValidator.ValidateAsync(new ValidationContext<SaveProposalsCommand>(command), ct);
        if (!vr.IsValid)
        {
            throw new ValidationException("Validation failed", vr.Errors);
        }

        var flashcards = command.Proposals
            .Where(p => p.IsAccepted && !p.IsRejected)
            .Select(p => FlashcardMapper.ToEntity(p, userId))
            .ToList();

        if (flashcards.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        _dbContext.Flashcards.AddRange(flashcards);
        await _dbContext.SaveChangesAsync(ct);

        return flashcards.Select(f => f.Id).ToList();
    }

    private async Task<IReadOnlyList<FlashcardProposalDto>> GenerateFlashcardsWithOpenRouter(
        StartGenerationCommand command, 
        CancellationToken ct = default)
    {
        try
        {
            // Create a chat session for flashcard generation using OpenAI GPT-4o mini
            var session = _openRouterService.CreateSession("openai/gpt-4o-mini");
            
            // Set system message for flashcard generation
            var systemPrompt = $"""
                You are an expert in creating educational flashcards for Polish language learning.
                Generate exactly {command.MaxCards} flashcards about the topic: "{command.SourceText}".
                
                Each flashcard should have:
                - Front: A Polish word, phrase, or question
                - Back: The English translation, definition, or answer
                - Example: A practical example sentence in Polish with English translation
                
                Focus on practical, commonly used vocabulary and phrases.
                Ensure variety in difficulty levels and content types.
                """;

            _openRouterService.SetSystemMessage(session, systemPrompt);

            // Add user prompt
            var userPrompt = $"""
                Create {command.MaxCards} Polish language flashcards about "{command.SourceText}".
                Include a mix of vocabulary, phrases, and practical expressions.
                Make them educational and useful for language learners.
                """;

            _openRouterService.AddUserMessage(session, userPrompt);

            // Define JSON schema for structured response
            var schema = new JsonSchemaDescriptor
            {
                Name = "flashcard_generation_response",
                SchemaObject = new
                {
                    type = "object",
                    properties = new
                    {
                        flashcards = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    front = new { type = "string", description = "Polish word, phrase or question" },
                                    back = new { type = "string", description = "English translation, definition or answer" },
                                    example = new { type = "string", description = "Example sentence in Polish with English translation" }
                                },
                                required = new[] { "front", "back", "example" },
                                additionalProperties = false
                            },
                            minItems = 1,
                            maxItems = command.MaxCards
                        }
                    },
                    required = new[] { "flashcards" },
                    additionalProperties = false
                }
            };

            // Configure completion options
            var options = _openRouterService.CreateOptions(builder =>
                builder.WithTemperature(0.7)
                       .WithMaxTokens(2000)
                       .WithStructuredSchema(schema));

            // Generate flashcards using structured completion
            var result = await _openRouterService.CompleteStructuredAsync<FlashcardGenerationResponse>(
                session, schema, options, ct);

            // Map to FlashcardProposalDto
            var proposals = result.Data.Flashcards.Select(fc => new FlashcardProposalDto(
                Guid.NewGuid(),
                fc.Front,
                fc.Back,
                fc.Example,
                null // No image URL for now
            )).ToList();

            _logger.LogInformation("Generated {Count} flashcards using OpenRouter for topic: {Topic}", 
                proposals.Count, command.SourceText);

            return proposals;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter flashcard generation failed for topic: {Topic}", command.SourceText);
            
            // Fallback to mock generation if OpenRouter fails
            _logger.LogWarning("Falling back to mock generation for topic: {Topic}", command.SourceText);
            return GenerateMockProposals(command);
        }
    }

    private static IReadOnlyList<FlashcardProposalDto> GenerateMockProposals(StartGenerationCommand command)
    {
        // Fallback mock generation for when OpenRouter is unavailable
        var proposals = new List<FlashcardProposalDto>();
        for (var i = 0; i < command.MaxCards; i++)
        {
            proposals.Add(new FlashcardProposalDto(
                Guid.NewGuid(),
                $"Polish Word {i + 1}",
                $"English Translation {i + 1}",
                $"Example sentence {i + 1} in Polish with English translation",
                null
            ));
        }
        return proposals;
    }

    private class GenerationJob
    {
        public required Guid Id { get; init; }
        public required DateTime QueuedAt { get; init; }
        public GenerationStatusEnum Status { get; set; }
        public required StartGenerationCommand Command { get; init; }
        public IReadOnlyList<FlashcardProposalDto>? Proposals { get; set; }
        public string? Error { get; set; }
    }
}

/// <summary>
/// Response model for structured flashcard generation from OpenRouter
/// </summary>
public class FlashcardGenerationResponse
{
    public required IReadOnlyList<GeneratedFlashcard> Flashcards { get; init; }
}

/// <summary>
/// Individual flashcard generated by OpenRouter
/// </summary>
public class GeneratedFlashcard
{
    public required string Front { get; init; }
    public required string Back { get; init; }
    public required string Example { get; init; }
}
