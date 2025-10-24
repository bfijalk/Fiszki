using Fiszki.Services.Commands;
using Fiszki.Services.Models.Generation;

namespace Fiszki.Services.Interfaces;

public interface IGenerationService
{
    Task<GenerationJobDto> StartAsync(StartGenerationCommand command, CancellationToken ct = default);
    Task<GenerationStatusDto> GetStatusAsync(Guid jobId, CancellationToken ct = default);
    Task CancelAsync(Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> SaveProposalsAsync(Guid userId, SaveProposalsCommand command, CancellationToken ct = default);
}
