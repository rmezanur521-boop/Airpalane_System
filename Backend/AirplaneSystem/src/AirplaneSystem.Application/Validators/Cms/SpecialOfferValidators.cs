using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreateSpecialOfferValidator : AbstractValidator<CreateSpecialOfferDto>
{
    public CreateSpecialOfferValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PromoCode).MaximumLength(30);
        RuleFor(x => x)
            .Must(x => x.EndDate == null || x.StartDate == null || x.EndDate >= x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public class UpdateSpecialOfferValidator : AbstractValidator<UpdateSpecialOfferDto>
{
    public UpdateSpecialOfferValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.EndDate == null || x.StartDate == null || x.EndDate >= x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}