using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreateFleetItemValidator : AbstractValidator<CreateFleetItemDto>
{
    public CreateFleetItemValidator()
    {
        RuleFor(x => x.AircraftName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(150);
        RuleFor(x => x.SeatCapacity).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateFleetItemValidator : AbstractValidator<UpdateFleetItemDto>
{
    public UpdateFleetItemValidator()
    {
        RuleFor(x => x.AircraftName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(150);
        RuleFor(x => x.SeatCapacity).GreaterThan(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}