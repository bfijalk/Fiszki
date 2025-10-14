using FluentValidation;
using Fiszki.Services.Commands;

namespace Fiszki.Services.Validation;

public class SaveProposalsCommandValidator : AbstractValidator<SaveProposalsCommand>
{
    public SaveProposalsCommandValidator()
    {
        RuleFor(x => x.Proposals)
            .NotEmpty()
            .WithMessage("At least one proposal must be selected")
            .Must(list => list.Any(p => p.IsAccepted && !p.IsRejected))
            .WithMessage("At least one proposal must be accepted");
            
        RuleForEach(x => x.Proposals)
            .ChildRules(proposal =>
            {
                proposal.RuleFor(p => p.Front)
                    .NotEmpty()
                    .WithMessage("Front side is required")
                    .MaximumLength(500)
                    .WithMessage("Front side must not exceed 500 characters");
                    
                proposal.RuleFor(p => p.Back)
                    .NotEmpty()
                    .WithMessage("Back side is required")
                    .MaximumLength(500)
                    .WithMessage("Back side must not exceed 500 characters");
                    
                proposal.RuleFor(p => p.Example)
                    .MaximumLength(1000)
                    .WithMessage("Example must not exceed 1000 characters")
                    .When(p => p.Example != null);
                    
                proposal.RuleFor(p => p.Notes)
                    .MaximumLength(1000)
                    .WithMessage("Notes must not exceed 1000 characters")
                    .When(p => p.Notes != null);
            });
    }
}
