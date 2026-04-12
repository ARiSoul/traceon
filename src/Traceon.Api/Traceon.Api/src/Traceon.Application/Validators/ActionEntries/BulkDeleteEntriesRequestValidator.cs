using FluentValidation;
using Traceon.Contracts.ActionEntries;

namespace Traceon.Application.Validators.ActionEntries;

public sealed class BulkDeleteEntriesRequestValidator : AbstractValidator<BulkDeleteEntriesRequest>
{
    public BulkDeleteEntriesRequestValidator()
    {
        RuleFor(x => x.EntryIds)
            .NotEmpty()
            .Must(ids => ids.Count <= 100)
            .WithMessage("Cannot delete more than 100 entries at once.");

        RuleForEach(x => x.EntryIds)
            .NotEmpty();
    }
}
