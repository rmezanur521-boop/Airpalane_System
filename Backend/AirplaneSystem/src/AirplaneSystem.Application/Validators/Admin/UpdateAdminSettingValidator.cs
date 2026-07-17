using AirplaneSystem.Application.DTOs.Admin;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Admin;

public class UpdateAdminSettingValidator : AbstractValidator<UpdateAdminSettingDto>
{
    public UpdateAdminSettingValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.SupportEmail)
            .NotEmpty().WithMessage("Support email is required.")
            .EmailAddress().WithMessage("Support email must be a valid email address.")
            .MaximumLength(200);

        RuleFor(x => x.SupportPhone)
            .NotEmpty().WithMessage("Support phone is required.")
            .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("Support phone format is invalid.");

        RuleFor(x => x.CompanyAddress)
            .NotEmpty().WithMessage("Company address is required.")
            .MaximumLength(500);

        RuleFor(x => x.WebsiteUrl)
            .NotEmpty().WithMessage("Website URL is required.")
            .Must(BeAValidUrl).WithMessage("Website URL must be a valid absolute URL (e.g. https://example.com).")
            .MaximumLength(300);

        RuleFor(x => x.FooterText)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.FooterText));
    }

    private static bool BeAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var result)
        && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
}