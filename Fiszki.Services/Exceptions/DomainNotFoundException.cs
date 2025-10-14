namespace Fiszki.Services.Exceptions;

public class DomainNotFoundException : Exception
{
    public DomainNotFoundException(string message) : base(message) { }
}

