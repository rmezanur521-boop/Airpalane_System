using AirplaneSystem.Application.DTOs.Admin;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Admin;

public class UpdateSmtpSettingValidator : AbstractValidator<UpdateSmtpSettingDto>
{
    public UpdateSmtpSettingValidator()
    {
        RuleFor(x => x.SmtpHost)
            .NotEmpty().WithMessage("SMTP host is required.")
            .MaximumLength(200);

        RuleFor(x => x.SmtpPort)
            .InclusiveBetween(1, 65535).WithMessage("SMTP port must be between 1 and 65535.");

        RuleFor(x => x.SmtpUsername)
            .NotEmpty().WithMessage("SMTP username is required.")
            .MaximumLength(200);

        // Password ঐচ্ছিক — দিলে Minimum Length Check করা হবে
        RuleFor(x => x.SmtpPassword)
            .MinimumLength(4).WithMessage("SMTP password looks too short.")
            .When(x => !string.IsNullOrEmpty(x.SmtpPassword));

        RuleFor(x => x.SmtpFromName)
            .NotEmpty().WithMessage("SMTP sender name is required.")
            .MaximumLength(200);

        RuleFor(x => x.SmtpFromEmail)
            .NotEmpty().WithMessage("SMTP sender email is required.")
            .EmailAddress().WithMessage("SMTP sender email must be a valid email address.")
            .MaximumLength(200);
    }
}