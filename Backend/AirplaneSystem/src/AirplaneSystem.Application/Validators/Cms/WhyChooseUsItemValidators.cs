using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreateWhyChooseUsItemValidator : AbstractValidator<CreateWhyChooseUsItemDto>
{
    public CreateWhyChooseUsItemValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Icon).MaximumLength(100);
        RuleFor(x => x.IconColor).MaximumLength(20);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateWhyChooseUsItemValidator : AbstractValidator<UpdateWhyChooseUsItemDto>
{
    public UpdateWhyChooseUsItemValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Icon).MaximumLength(100);
        RuleFor(x => x.IconColor).MaximumLength(20);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}