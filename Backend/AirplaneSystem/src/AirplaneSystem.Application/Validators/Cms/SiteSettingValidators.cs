using AirplaneSystem.Application.DTOs.Cms;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Cms;

public class UpdateNavbarSettingValidator : AbstractValidator<UpdateNavbarSettingDto>
{
    public UpdateNavbarSettingValidator()
    {
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SupportEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.SupportEmail));
        RuleFor(x => x.SupportPhone).MaximumLength(30);
    }
}

public class UpdateFooterSettingValidator : AbstractValidator<UpdateFooterSettingDto>
{
    public UpdateFooterSettingValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Facebook).MaximumLength(300);
        RuleFor(x => x.Instagram).MaximumLength(300);
        RuleFor(x => x.Youtube).MaximumLength(300);
        RuleFor(x => x.LinkedIn).MaximumLength(300);
        RuleFor(x => x.Twitter).MaximumLength(300);
    }
}

// HomepageSetting শুধু bool Field, তাই Validator-এর দরকার নেই —
// তবু FluentValidation Consistency-র জন্য একটা Empty Validator রাখছি
public class UpdateHomepageSettingValidator : AbstractValidator<UpdateHomepageSettingDto>
{
    public UpdateHomepageSettingValidator() { }
}