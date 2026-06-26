using AirplaneSystem.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirplaneSystem.Infrastructure.ExternalServices.Pdf;

public class QuestPdfService : IPdfService
{
    public QuestPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateTicketPdfAsync(TicketPdfModel model, CancellationToken ct = default)
    {
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(h => BuildHeader(h, model.AirlineName));
                page.Content().Element(c => BuildContent(c, model));
                page.Footer().Element(f => BuildFooter(f, model));
            });
        }).GeneratePdf();

        return Task.FromResult(bytes);
    }

    public Task<byte[]> GenerateBoardingPassPdfAsync(BoardingPassModel model, CancellationToken ct = default)
    {
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Element(c => BuildBoardingPass(c, model));
            });
        }).GeneratePdf();

        return Task.FromResult(bytes);
    }

    private static void BuildHeader(IContainer container, string airlineName)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(airlineName).FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                col.Item().Text("Electronic Ticket").FontSize(14).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void BuildContent(IContainer container, TicketPdfModel model)
    {
        container.PaddingTop(20).Column(col =>
        {
            // Booking info
            col.Item().Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
            {
                row.RelativeItem().Text($"Booking Reference: {model.BookingReference}").Bold();
                row.RelativeItem().Text($"Ticket No: {model.TicketNumber}").Bold().AlignRight();
            });

            col.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(cols => { cols.RelativeColumn(); cols.RelativeColumn(); });
                table.Cell().Padding(5).Text("Passenger Name").SemiBold();
                table.Cell().Padding(5).Text(model.PassengerName);
                table.Cell().Padding(5).Text("Flight Number").SemiBold();
                table.Cell().Padding(5).Text(model.FlightNumber);
                table.Cell().Padding(5).Text("From").SemiBold();
                table.Cell().Padding(5).Text($"{model.OriginCity} ({model.OriginIata})");
                table.Cell().Padding(5).Text("To").SemiBold();
                table.Cell().Padding(5).Text($"{model.DestinationCity} ({model.DestinationIata})");
                table.Cell().Padding(5).Text("Departure").SemiBold();
                table.Cell().Padding(5).Text(model.DepartureTime.ToString("dd MMM yyyy HH:mm") + " UTC");
                table.Cell().Padding(5).Text("Arrival").SemiBold();
                table.Cell().Padding(5).Text(model.ArrivalTime.ToString("dd MMM yyyy HH:mm") + " UTC");
                table.Cell().Padding(5).Text("Class").SemiBold();
                table.Cell().Padding(5).Text(model.SeatClass);
                table.Cell().Padding(5).Text("Seat").SemiBold();
                table.Cell().Padding(5).Text(model.SeatNumber ?? "To be assigned");
                table.Cell().Padding(5).Text("Total Amount").SemiBold();
                table.Cell().Padding(5).Text($"USD {model.TotalAmount:F2}").Bold();
            });
        });
    }

    private static void BuildFooter(IContainer container, TicketPdfModel model)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Please present this ticket at check-in.").FontSize(8).FontColor(Colors.Grey.Medium);
                col.Item().Text("This is an electronic ticket. No physical ticket required.").FontSize(8).FontColor(Colors.Grey.Medium);
            });

            if (model.QrCodeBytes != null)
            {
                row.ConstantItem(80).Image(model.QrCodeBytes);
            }
        });
    }

    private static void BuildBoardingPass(IContainer container, BoardingPassModel model)
    {
        container.Row(row =>
        {
            row.RelativeItem(3).Column(col =>
            {
                col.Item().Text(model.AirlineName).FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                col.Item().PaddingTop(5).Text("BOARDING PASS").FontSize(12).Bold();
                col.Item().PaddingTop(10).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("FROM").FontSize(8).FontColor(Colors.Grey.Medium);
                        c.Item().Text(model.OriginIata).FontSize(24).Bold();
                        c.Item().Text(model.OriginCity).FontSize(9);
                    });
                    r.ConstantItem(40).AlignMiddle().Text("→").FontSize(20);
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("TO").FontSize(8).FontColor(Colors.Grey.Medium);
                        c.Item().Text(model.DestinationIata).FontSize(24).Bold();
                        c.Item().Text(model.DestinationCity).FontSize(9);
                    });
                });
                col.Item().PaddingTop(10).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); });
                    t.Cell().Padding(3).Column(c => { c.Item().Text("PASSENGER").FontSize(7).FontColor(Colors.Grey.Medium); c.Item().Text(model.PassengerName).FontSize(9).Bold(); });
                    t.Cell().Padding(3).Column(c => { c.Item().Text("FLIGHT").FontSize(7).FontColor(Colors.Grey.Medium); c.Item().Text(model.FlightNumber).FontSize(9).Bold(); });
                    t.Cell().Padding(3).Column(c => { c.Item().Text("SEAT").FontSize(7).FontColor(Colors.Grey.Medium); c.Item().Text(model.SeatNumber ?? "TBD").FontSize(9).Bold(); });
                    t.Cell().Padding(3).Column(c => { c.Item().Text("CLASS").FontSize(7).FontColor(Colors.Grey.Medium); c.Item().Text(model.SeatClass).FontSize(9).Bold(); });
                    t.Cell().Padding(3).Column(c => { c.Item().Text("GATE").FontSize(7).FontColor(Colors.Grey.Medium); c.Item().Text(model.Gate).FontSize(9).Bold(); });
                    t.Cell().Padding(3).Column(c => { c.Item().Text("DEPARTURE").FontSize(7).FontColor(Colors.Grey.Medium); c.Item().Text(model.DepartureTime.ToString("HH:mm")).FontSize(9).Bold(); });
                });
            });

            if (model.QrCodeBytes != null)
            {
                row.ConstantItem(100).AlignMiddle().Image(model.QrCodeBytes);
            }
        });
    }
}
