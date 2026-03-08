using FluentValidation;
using Traceon.Contracts.ActionEntries;

namespace Traceon.Application.Validators.ActionEntries;

public sealed class CreateActionEntryRequestValidator : AbstractValidator<CreateActionEntryRequest>
{
    public CreateActionEntryRequestValidator()
    {
        RuleFor(x => x.OccurredAtUtc)
            .NotEmpty();

        RuleForEach(x => x.FieldValues)
            .ChildRules(fv =>
            {
                fv.RuleFor(x => x.ActionFieldId).NotEmpty();
            })
            .When(x => x.FieldValues is not null);
    }
}
