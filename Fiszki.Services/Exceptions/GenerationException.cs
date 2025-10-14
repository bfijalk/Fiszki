using Fiszki.Services.Models.Generation;

namespace Fiszki.Services.Exceptions;

public class GenerationException : Exception
{
    public GenerationStatusEnum Status { get; }
    
    public GenerationException(string message, GenerationStatusEnum status) 
        : base(message)
    {
        Status = status;
    }
    
    public GenerationException(string message, GenerationStatusEnum status, Exception inner) 
        : base(message, inner)
    {
        Status = status;
    }
}
