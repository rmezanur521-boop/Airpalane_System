using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreateTravelServiceValidator : AbstractValidator<CreateTravelServiceDto>
{
    public CreateTravelServiceValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.RedirectUrl).MaximumLength(500);
        RuleFor(x => x.ButtonText).MaximumLength(50);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateTravelServiceValidator : AbstractValidator<UpdateTravelServiceDto>
{
    public UpdateTravelServiceValidator()
    {
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.RedirectUrl).MaximumLength(500);
        RuleFor(x => x.ButtonText).MaximumLength(50);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}