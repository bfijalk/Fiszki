using FluentValidation;
using Fiszki.Services.Commands;

namespace Fiszki.Services.Validation;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        // Relaxed: only ensure not empty and length cap (DB column citext required + unique)
        RuleFor(x => x.Email)
            .NotEmpty().MaximumLength(255);
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(3).MaximumLength(128)
            .Matches("[0-9]").WithMessage("Password must contain digit");
    }
}
