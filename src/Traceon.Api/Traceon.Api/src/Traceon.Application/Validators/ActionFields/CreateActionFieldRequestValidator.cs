using FluentValidation;
using Traceon.Application.Contracts.ActionFields;

namespace Traceon.Application.Validators.ActionFields;

public sealed class CreateActionFieldRequestValidator : AbstractValidator<CreateActionFieldRequest>
{
    public CreateActionFieldRequestValidator()
    {
        RuleFor(x => x.FieldDefinitionId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.MinValue)
            .LessThanOrEqualTo(x => x.MaxValue)
            .When(x => x.MinValue.HasValue && x.MaxValue.HasValue)
            .WithMessage("MinValue must be less than or equal to MaxValue.");
    }
}
