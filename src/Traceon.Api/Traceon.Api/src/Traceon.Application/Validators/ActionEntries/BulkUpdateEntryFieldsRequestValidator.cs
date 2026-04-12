using FluentValidation;
using Traceon.Contracts.ActionEntries;

namespace Traceon.Application.Validators.ActionEntries;

public sealed class BulkUpdateEntryFieldsRequestValidator : AbstractValidator<BulkUpdateEntryFieldsRequest>
{
    public BulkUpdateEntryFieldsRequestValidator()
    {
        RuleFor(x => x.EntryIds)
            .NotEmpty()
            .Must(ids => ids.Count <= 100)
            .WithMessage("Cannot update more than 100 entries at once.");

        RuleForEach(x => x.EntryIds)
            .NotEmpty();

        RuleFor(x => x.FieldValues)
            .NotEmpty();

        RuleForEach(x => x.FieldValues)
            .ChildRules(fv =>
            {
                fv.RuleFor(x => x.ActionFieldId).NotEmpty();
            });
    }
}
