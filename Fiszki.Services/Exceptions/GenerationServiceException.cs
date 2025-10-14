using Fiszki.Services.Models.Generation;

namespace Fiszki.Services.Exceptions;

public class GenerationServiceException : Exception
{
    public GenerationStatusEnum Status { get; }

    public GenerationServiceException(string message, GenerationStatusEnum status = GenerationStatusEnum.Failed) 
        : base(message)
    {
        Status = status;
    }

    public static GenerationServiceException JobNotFound(Guid jobId)
        => new($"Generation job {jobId} not found", GenerationStatusEnum.Failed);

    public static GenerationServiceException ValidationFailed(string message)
        => new($"Validation failed: {message}", GenerationStatusEnum.Failed);
}
