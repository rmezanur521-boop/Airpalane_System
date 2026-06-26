using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.Exceptions;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork uow, IEmailService email, ISmsService sms,
        IConfiguration config, ILogger<NotificationService> logger)
    {
        _uow = uow;
        _email = email;
        _sms = sms;
        _config = config;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetWithDetailsAsync(bookingId, ct);
        if (booking == null) return;

        var user = await _uow.Users.GetByIdAsync(booking.UserId, ct);
        if (user == null) return;

        var html = $@"
<h2>Booking Confirmed!</h2>
<p>Dear {user.FirstName},</p>
<p>Your booking <strong>{booking.BookingReference}</strong> has been confirmed.</p>
<p>Total Amount: <strong>${booking.TotalAmount:F2} USD</strong></p>
<p>Thank you for booking with us.</p>";

        await _email.SendAsync(user.Email, $"Booking Confirmed - {booking.BookingReference}", html, ct);

        if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            try
            {
                await _sms.SendAsync(user.PhoneNumber,
                    $"AirSystem: Your booking {booking.BookingReference} is confirmed. Amount: ${booking.TotalAmount:F2} USD", ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "SMS failed for booking {Id}", bookingId); }
        }
    }

    public async Task SendPaymentReceiptAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await _uow.Payments.GetByIdAsync(paymentId, ct);
        if (payment == null) return;

        var booking = await _uow.Bookings.GetByIdAsync(payment.BookingId, ct);
        if (booking == null) return;

        var user = await _uow.Users.GetByIdAsync(booking.UserId, ct);
        if (user == null) return;

        var html = $@"
<h2>Payment Receipt</h2>
<p>Dear {user.FirstName},</p>
<p>Payment of <strong>${payment.Amount:F2} USD</strong> received for booking <strong>{booking.BookingReference}</strong>.</p>
<p>Transaction ID: {payment.StripePaymentIntentId}</p>
{(payment.ReceiptUrl != null ? $"<p><a href='{payment.ReceiptUrl}'>View Receipt</a></p>" : "")}";

        await _email.SendAsync(user.Email, $"Payment Received - {booking.BookingReference}", html, ct);
    }

    public async Task SendFlightDelayAlertAsync(Guid flightId, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetWithDetailsAsync(flightId, ct);
        if (flight == null) return;

        _logger.LogInformation("Sending delay alerts for flight {FlightNumber}", flight.FlightNumber);
        var html = $@"
<h2>Flight Delay Notice</h2>
<p>Flight <strong>{flight.FlightNumber}</strong> has been delayed.</p>
<p>Route: {flight.Route.OriginAirport.IataCode} → {flight.Route.DestinationAirport.IataCode}</p>
<p>We apologize for any inconvenience.</p>";

        await NotifyBookedPassengersAsync(flightId, $"Flight Delay - {flight.FlightNumber}", html, ct);
    }

    public async Task SendFlightCancellationAlertAsync(Guid flightId, CancellationToken ct = default)
    {
        var flight = await _uow.Flights.GetWithDetailsAsync(flightId, ct);
        if (flight == null) return;

        var html = $@"
<h2>Flight Cancellation Notice</h2>
<p>We regret to inform you that flight <strong>{flight.FlightNumber}</strong> has been cancelled.</p>
<p>Our team will contact you regarding rebooking options or a full refund.</p>";

        await NotifyBookedPassengersAsync(flightId, $"Flight Cancelled - {flight.FlightNumber}", html, ct);
    }

    public async Task SendEmailVerificationAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null) return;

        var baseUrl = _config["AppSettings:FrontendUrl"] ?? "https://airsystem.com";
        var verifyUrl = $"{baseUrl}/verify-email?token={user.EmailVerificationToken}&email={Uri.EscapeDataString(user.Email)}";

        var html = $@"
<h2>Verify Your Email</h2>
<p>Dear {user.FirstName},</p>
<p>Thank you for registering. Please verify your email address:</p>
<p><a href='{verifyUrl}' style='background:#0066cc;color:white;padding:10px 20px;text-decoration:none;border-radius:4px;'>Verify Email</a></p>
<p>This link expires in 24 hours.</p>
<p>If you did not register, please ignore this email.</p>";

        await _email.SendAsync(user.Email, "Verify Your Email - AirSystem", html, ct);
    }

    public async Task SendPasswordResetAsync(Guid userId, string resetToken, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user == null) return;

        var baseUrl = _config["AppSettings:FrontendUrl"] ?? "https://airsystem.com";
        var resetUrl = $"{baseUrl}/reset-password?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";

        var html = $@"
<h2>Password Reset Request</h2>
<p>Dear {user.FirstName},</p>
<p>We received a request to reset your password. Click below to reset it:</p>
<p><a href='{resetUrl}' style='background:#dc3545;color:white;padding:10px 20px;text-decoration:none;border-radius:4px;'>Reset Password</a></p>
<p>This link expires in 2 hours. If you did not request this, please ignore this email.</p>";

        await _email.SendAsync(user.Email, "Password Reset - AirSystem", html, ct);
    }

    public async Task SendBookingExpiryNoticeAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _uow.Bookings.GetByIdAsync(bookingId, ct);
        if (booking == null) return;
        var user = await _uow.Users.GetByIdAsync(booking.UserId, ct);
        if (user == null) return;

        var html = $@"
<h2>Booking Hold Expired</h2>
<p>Dear {user.FirstName},</p>
<p>Your booking hold <strong>{booking.BookingReference}</strong> has expired because payment was not completed within 15 minutes.</p>
<p>Please start a new booking if you still wish to travel.</p>";

        await _email.SendAsync(user.Email, $"Booking Expired - {booking.BookingReference}", html, ct);
    }

    private async Task NotifyBookedPassengersAsync(Guid flightId, string subject, string htmlBody, CancellationToken ct)
    {
        var bookings = await _uow.Bookings.FindAsync(
            b => b.BookingSegments.Any(s => s.FlightId == flightId) &&
                 (b.Status == Domain.Enums.BookingStatus.Confirmed || b.Status == Domain.Enums.BookingStatus.PendingPayment), ct);

        foreach (var booking in bookings)
        {
            var user = await _uow.Users.GetByIdAsync(booking.UserId, ct);
            if (user == null) continue;
            try { await _email.SendAsync(user.Email, subject, htmlBody, ct); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to send notification to {Email}", user.Email); }
        }
    }
}
