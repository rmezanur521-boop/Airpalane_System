using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class CreateAnnouncementBarValidator : AbstractValidator<CreateAnnouncementBarDto>
{
    public CreateAnnouncementBarValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BackgroundColor).MaximumLength(20);
        RuleFor(x => x.TextColor).MaximumLength(20);
        RuleFor(x => x)
            .Must(x => x.EndDate == null || x.StartDate == null || x.EndDate >= x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}

public class UpdateAnnouncementBarValidator : AbstractValidator<UpdateAnnouncementBarDto>
{
    public UpdateAnnouncementBarValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BackgroundColor).MaximumLength(20);
        RuleFor(x => x.TextColor).MaximumLength(20);
        RuleFor(x => x)
            .Must(x => x.EndDate == null || x.StartDate == null || x.EndDate >= x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}