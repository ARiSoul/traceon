using FluentValidation;
using Traceon.Application.Contracts.Tags;

namespace Traceon.Application.Validators.Tags;

public sealed class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.Color)
            .NotEmpty()
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a valid hex color (e.g., #FF5733).");
    }
}
