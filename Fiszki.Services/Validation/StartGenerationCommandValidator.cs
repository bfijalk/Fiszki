using FluentValidation;
using Fiszki.Services.Commands;

namespace Fiszki.Services.Validation;

public class StartGenerationCommandValidator : AbstractValidator<StartGenerationCommand>
{
    private static readonly string[] SupportedLanguages = { "en", "pl", "es", "de", "fr" };
    
    public StartGenerationCommandValidator()
    {
        RuleFor(x => x.SourceText)
            .NotEmpty()
            .WithMessage("Source text is required")
            .MinimumLength(50)
            .WithMessage("Source text must be at least 50 characters long")
            .MaximumLength(5000)
            .WithMessage("Source text must not exceed 5000 characters");
            
        RuleFor(x => x.Language)
            .NotEmpty()
            .WithMessage("Language code is required")
            .Must(code => SupportedLanguages.Contains(code.ToLowerInvariant()))
            .WithMessage("Unsupported language code. Supported languages: en, pl, es, de, fr");
            
        RuleFor(x => x.MaxCards)
            .InclusiveBetween(1, 50)
            .WithMessage("Number of cards must be between 1 and 50");
    }
}
