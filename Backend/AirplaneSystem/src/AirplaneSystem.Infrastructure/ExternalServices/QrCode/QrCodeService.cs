using AirplaneSystem.Application.Common.Interfaces;
using QRCoder;
using System.Text.Json;

namespace AirplaneSystem.Infrastructure.ExternalServices.QrCode;

public class QrCodeService : IQrCodeService
{
    public Task<byte[]> GenerateAsync(string data, CancellationToken ct = default)
    {
        using var generator = new QRCodeGenerator();
        var qrData = generator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return Task.FromResult(qrCode.GetGraphic(10));
    }

    public string BuildTicketQrPayload(string ticketNumber, string bookingRef, string flightNumber,
        string origin, string destination, string seatNumber, string passengerName,
        string seatClass, DateTime departureTime)
    {
        var payload = new
        {
            tn = ticketNumber,
            br = bookingRef,
            fn = flightNumber,
            dep = origin,
            arr = destination,
            seat = seatNumber,
            pax = passengerName,
            cls = seatClass,
            dep_time = departureTime.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        return JsonSerializer.Serialize(payload);
    }
}
