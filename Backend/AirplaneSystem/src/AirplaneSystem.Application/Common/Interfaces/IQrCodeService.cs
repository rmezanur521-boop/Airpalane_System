namespace AirplaneSystem.Application.Common.Interfaces;

public interface IQrCodeService
{
    Task<byte[]> GenerateAsync(string data, CancellationToken ct = default);
    string BuildTicketQrPayload(string ticketNumber, string bookingRef, string flightNumber,
        string origin, string destination, string seatNumber, string passengerName,
        string seatClass, DateTime departureTime);
}
