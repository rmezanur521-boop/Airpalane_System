namespace AirplaneSystem.Application.Services.Interfaces;

public interface INotificationService
{
    Task SendBookingConfirmationAsync(Guid bookingId, CancellationToken ct = default);
    Task SendPaymentReceiptAsync(Guid paymentId, CancellationToken ct = default);
    Task SendFlightDelayAlertAsync(Guid flightId, CancellationToken ct = default);
    Task SendEmailVerificationAsync(Guid userId, CancellationToken ct = default);
    Task SendPasswordResetAsync(Guid userId, string resetToken, CancellationToken ct = default);
    Task SendBookingExpiryNoticeAsync(Guid bookingId, CancellationToken ct = default);
    Task SendFlightCancellationAlertAsync(Guid flightId, CancellationToken ct = default);
}
