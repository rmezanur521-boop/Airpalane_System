using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreatePopularDestinationValidator : AbstractValidator<CreatePopularDestinationDto>
{
    public CreatePopularDestinationValidator()
    {
        RuleFor(x => x.DestinationName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdatePopularDestinationValidator : AbstractValidator<UpdatePopularDestinationDto>
{
    public UpdatePopularDestinationValidator()
    {
        RuleFor(x => x.DestinationName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}