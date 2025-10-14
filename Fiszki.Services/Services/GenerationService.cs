using Fiszki.Services.Commands;
using Fiszki.Services.Interfaces;
using Fiszki.Services.Models.Generation;
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
    private readonly ConcurrentDictionary<Guid, GenerationJob> _jobs = new();

    public GenerationService(
        ILogger<GenerationService> logger,
        IValidator<StartGenerationCommand> startValidator,
        IValidator<SaveProposalsCommand> saveValidator,
        FiszkiDbContext dbContext)
    {
        _logger = logger;
        _startValidator = startValidator;
        _saveValidator = saveValidator;
        _dbContext = dbContext;
    }

    public async Task<GenerationJobDto> StartAsync(StartGenerationCommand command, CancellationToken ct = default)
    {
        await _startValidator.ValidateAndThrowAsync(command, ct);

        var job = new GenerationJob
        {
            Id = Guid.NewGuid(),
            QueuedAt = DateTime.UtcNow,
            Status = GenerationStatusEnum.Queued,
            Command = command
        };

        _jobs.TryAdd(job.Id, job);
        
        // TODO: Replace with actual AI integration
        _ = Task.Run(async () =>
        {
            try
            {
                job.Status = GenerationStatusEnum.Generating;
                await Task.Delay(3000); // Simulating AI processing
                
                job.Proposals = GenerateMockProposals(command);
                job.Status = GenerationStatusEnum.Completed;
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

    public async Task<IReadOnlyList<Guid>> SaveProposalsAsync(SaveProposalsCommand command, CancellationToken ct = default)
    {
        await _saveValidator.ValidateAndThrowAsync(command, ct);

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // TODO: Get from auth context
        var flashcards = command.Proposals
            .Where(p => p.IsAccepted && !p.IsRejected)
            .Select(p => FlashcardMapper.ToEntity(p, userId))
            .ToList();

        _dbContext.Flashcards.AddRange(flashcards);
        await _dbContext.SaveChangesAsync(ct);

        return flashcards.Select(f => f.Id).ToList();
    }

    private static IReadOnlyList<FlashcardProposalDto> GenerateMockProposals(StartGenerationCommand command)
    {
        // TODO: Replace with actual AI-generated proposals
        var proposals = new List<FlashcardProposalDto>();
        for (var i = 0; i < command.MaxCards; i++)
        {
            proposals.Add(new FlashcardProposalDto(
                Guid.NewGuid(),
                $"Front {i + 1}",
                $"Back {i + 1}",
                $"Example {i + 1}",
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
