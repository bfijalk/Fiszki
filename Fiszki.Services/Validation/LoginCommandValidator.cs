using FluentValidation;
using Fiszki.Services.Commands;

namespace Fiszki.Services.Validation;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password)
            .NotEmpty().MaximumLength(128);
    }
}
