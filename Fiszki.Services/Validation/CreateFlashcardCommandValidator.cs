using FluentValidation;
using Fiszki.Services.Commands;

namespace Fiszki.Services.Validation;

public class CreateFlashcardCommandValidator : AbstractValidator<CreateFlashcardCommand>
{
    public CreateFlashcardCommandValidator()
    {
        RuleFor(x => x.FrontContent).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.BackContent).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.Tags!).MaximumLength(50);
        RuleFor(x => x.Tags).Must(t => t == null || t.Count <= 10)
            .WithMessage("Maximum 10 tags allowed");
    }
}

