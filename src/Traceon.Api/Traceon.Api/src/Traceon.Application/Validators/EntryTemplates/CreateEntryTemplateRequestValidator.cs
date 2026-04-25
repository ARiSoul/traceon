using FluentValidation;
using Traceon.Contracts.EntryTemplates;

namespace Traceon.Application.Validators.EntryTemplates;

public sealed class CreateEntryTemplateRequestValidator : AbstractValidator<CreateEntryTemplateRequest>
{
    public CreateEntryTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(500);

        RuleForEach(x => x.FieldValues)
            .ChildRules(fv =>
            {
                fv.RuleFor(x => x.ActionFieldId).NotEmpty();
            })
            .When(x => x.FieldValues is not null);
    }
}
