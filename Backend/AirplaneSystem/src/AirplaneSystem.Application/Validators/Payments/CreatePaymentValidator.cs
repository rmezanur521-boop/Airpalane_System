using AirplaneSystem.Application.DTOs.Payments;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Payments;

public class CreatePaymentIntentValidator : AbstractValidator<CreatePaymentIntentRequest>
{
    public CreatePaymentIntentValidator()
    {
        RuleFor(x => x.BookingId)
            .NotEmpty().WithMessage("Booking ID is required.");
    }
}

public class ConfirmPaymentValidator : AbstractValidator<ConfirmPaymentRequest>
{
    public ConfirmPaymentValidator()
    {
        RuleFor(x => x.PaymentIntentId)
            .NotEmpty().WithMessage("Payment intent ID is required.")
            .Matches("^pi_").WithMessage("Invalid payment intent ID format.");
    }
}

public class PromoValidationRequestValidator : AbstractValidator<PromoValidationRequest>
{
    public PromoValidationRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CartTotal).GreaterThan(0).WithMessage("Cart total must be greater than zero.");
    }
}
