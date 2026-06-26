using AirplaneSystem.Application.DTOs.Flights;
using FluentValidation;

namespace AirplaneSystem.Application.Validators.Flights;

public class FlightSearchValidator : AbstractValidator<FlightSearchRequest>
{
    public FlightSearchValidator()
    {
        RuleFor(x => x.OriginIata)
            .NotEmpty().WithMessage("Origin airport is required.")
            .Length(3).WithMessage("Origin IATA code must be 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("Origin IATA code must be uppercase letters.");

        RuleFor(x => x.DestinationIata)
            .NotEmpty().WithMessage("Destination airport is required.")
            .Length(3).WithMessage("Destination IATA code must be 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("Destination IATA code must be uppercase letters.");

        RuleFor(x => x)
            .Must(x => x.OriginIata != x.DestinationIata)
            .WithMessage("Origin and destination airports cannot be the same.");

        RuleFor(x => x.DepartureDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Departure date must be today or in the future.");

        RuleFor(x => x.Passengers.Adults)
            .GreaterThanOrEqualTo(1).WithMessage("At least 1 adult passenger is required.");

        RuleFor(x => x.Passengers.Total)
            .LessThanOrEqualTo(9).WithMessage("Maximum 9 passengers per booking.");

        RuleFor(x => x.Passengers.Infants)
            .LessThanOrEqualTo(x => x.Passengers.Adults)
            .WithMessage("Number of infants cannot exceed number of adults.");

        RuleFor(x => x.MaxStops)
            .InclusiveBetween(0, 3).WithMessage("Max stops must be between 0 and 3.");
    }
}
