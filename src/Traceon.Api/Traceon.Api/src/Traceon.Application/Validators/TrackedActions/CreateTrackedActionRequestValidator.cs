using FluentValidation;
using Traceon.Contracts.TrackedActions;

namespace Traceon.Application.Validators.TrackedActions;

public sealed class CreateTrackedActionRequestValidator : AbstractValidator<CreateTrackedActionRequest>
{
    public CreateTrackedActionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);
    }
}
