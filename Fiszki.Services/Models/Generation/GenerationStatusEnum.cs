namespace Fiszki.Services.Models.Generation;

public enum GenerationStatusEnum
{
    Idle,
    Queued,
    Generating,
    Completed,
    Failed,
    Canceled
}
