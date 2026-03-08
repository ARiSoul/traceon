using FluentValidation;
using Traceon.Application.Contracts.ActionEntries;

namespace Traceon.Application.Validators.ActionEntries;

public sealed class UpdateActionEntryRequestValidator : AbstractValidator<UpdateActionEntryRequest>
{
    public UpdateActionEntryRequestValidator()
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
