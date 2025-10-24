namespace Fiszki.Services.Models.Generation;

public record GenerationJobDto(
    Guid JobId,
    DateTime QueuedAt,
    GenerationStatusEnum Status
);
