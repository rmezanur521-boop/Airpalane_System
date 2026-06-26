using AirplaneSystem.Application.DTOs.Bookings;
using AirplaneSystem.Domain.Enums;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Bookings;

public class CreateBookingValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.Segments)
            .NotEmpty().WithMessage("At least one flight segment is required.")
            .Must(s => s.Count >= 1).WithMessage("At least one flight segment is required.");

        RuleFor(x => x.Passengers)
            .NotEmpty().WithMessage("At least one passenger is required.");

        RuleFor(x => x)
            .Must(x => x.Passengers.Count(p => p.PassengerType == PassengerType.Adult) >= 1)
            .WithMessage("At least one adult passenger is required.");

        RuleFor(x => x)
            .Must(x =>
            {
                var adults = x.Passengers.Count(p => p.PassengerType == PassengerType.Adult);
                var infants = x.Passengers.Count(p => p.PassengerType == PassengerType.Infant);
                return infants <= adults;
            })
            .WithMessage("Number of infants cannot exceed number of adults.");

        RuleForEach(x => x.Passengers).SetValidator(new PassengerRequestValidator());

        RuleForEach(x => x.Segments).SetValidator(new BookingSegmentRequestValidator());
    }
}

public class PassengerRequestValidator : AbstractValidator<PassengerRequest>
{
    public PassengerRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty();

        When(x => x.PassengerType == PassengerType.Adult || x.PassengerType == PassengerType.Child, () =>
        {
            RuleFor(x => x.PassportNumber)
                .NotEmpty().WithMessage("Passport number is required for adult/child passengers.");
            RuleFor(x => x.PassportExpiry)
                .NotNull().WithMessage("Passport expiry is required.")
                .Must(e => e > DateOnly.FromDateTime(DateTime.Today.AddMonths(6)))
                .WithMessage("Passport must be valid for at least 6 months from today.");
        });
    }
}

public class BookingSegmentRequestValidator : AbstractValidator<BookingSegmentRequest>
{
    public BookingSegmentRequestValidator()
    {
        RuleFor(x => x.FlightId).NotEmpty().WithMessage("Flight ID is required.");
    }
}
