using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreateHeroSectionValidator : AbstractValidator<CreateHeroSectionDto>
{
    public CreateHeroSectionValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subtitle).MaximumLength(500);
        RuleFor(x => x.ButtonText).MaximumLength(50);
        RuleFor(x => x.ButtonLink).MaximumLength(500);
        RuleFor(x => x.OverlayOpacity).InclusiveBetween(0, 1)
            .WithMessage("OverlayOpacity must be between 0 and 1.");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateHeroSectionValidator : AbstractValidator<UpdateHeroSectionDto>
{
    public UpdateHeroSectionValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Subtitle).MaximumLength(500);
        RuleFor(x => x.ButtonText).MaximumLength(50);
        RuleFor(x => x.ButtonLink).MaximumLength(500);
        RuleFor(x => x.OverlayOpacity).InclusiveBetween(0, 1);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}