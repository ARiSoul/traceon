using FluentValidation;
using Traceon.Contracts.FieldDefinitions;
using Traceon.Contracts.Enums;

namespace Traceon.Application.Validators.FieldDefinitions;

public sealed class UpdateFieldDefinitionRequestValidator : AbstractValidator<UpdateFieldDefinitionRequest>
{
    public UpdateFieldDefinitionRequestValidator()
    {
        RuleFor(x => x.DefaultName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.DefaultDescription)
            .MaximumLength(1000)
            .When(x => x.DefaultDescription is not null);

        RuleFor(x => x.DropdownValues)
            .NotEmpty()
            .When(x => x.Type == FieldType.Dropdown)
            .WithMessage("Dropdown values are required for Dropdown field type.");

        RuleFor(x => x.DefaultMinValue)
            .LessThanOrEqualTo(x => x.DefaultMaxValue)
            .When(x => x.DefaultMinValue.HasValue && x.DefaultMaxValue.HasValue)
            .WithMessage("DefaultMinValue must be less than or equal to DefaultMaxValue.");
    }
}
