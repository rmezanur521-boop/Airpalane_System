using AirplaneSystem.Application.Common.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirplaneSystem.Infrastructure.ExternalServices.Pdf;

public class QuestPdfService : IPdfService
{
    // Primary brand theme colors (matching system theme rgb(2, 127, 34))
    private static readonly Color PrimaryColor = Color.FromHex("#027f22");
    private static readonly Color PrimaryLight = Color.FromHex("#f0fdf4");
    private static readonly Color PrimaryBorder = Color.FromHex("#bbf7d0");
    private static readonly Color DarkText = Color.FromHex("#0f172a");
    private static readonly Color MutedText = Color.FromHex("#64748b");
    private static readonly Color CardBg = Color.FromHex("#f8fafc");
    private static readonly Color BorderColor = Color.FromHex("#e2e8f0");

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
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(DarkText));

                page.Header().Element(h => BuildTicketHeader(h, model));
                page.Content().Element(c => BuildTicketContent(c, model));
                page.Footer().Element(f => BuildTicketFooter(f, model));
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
                page.Margin(16);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(DarkText));

                page.Content().Element(c => BuildBoardingPass(c, model));
            });
        }).GeneratePdf();

        return Task.FromResult(bytes);
    }

    // ==========================================
    // PROTOTYPE 1: E-TICKET RECEIPT (A4 FORMAT)
    // ==========================================

    private static void BuildTicketHeader(IContainer container, TicketPdfModel model)
    {
        container.Column(col =>
        {
            // Top Accent Bar
            col.Item().Height(4).Background(PrimaryColor);

            col.Item().PaddingVertical(8).Row(row =>
            {
                // Left: Airline Brand
                row.RelativeItem(3).Row(logoRow =>
                {
                    if (model.AirlineLogoBytes != null && model.AirlineLogoBytes.Length > 0)
                    {
                        logoRow.ConstantItem(45).Height(45).AlignMiddle().Image(model.AirlineLogoBytes);
                        logoRow.ConstantItem(10);
                    }

                    logoRow.RelativeItem().Column(brandCol =>
                    {
                        brandCol.Item().Text(string.IsNullOrWhiteSpace(model.AirlineName) ? "AirSystem" : model.AirlineName)
                            .FontSize(18).Bold().FontColor(PrimaryColor);
                        brandCol.Item().Text("ELECTRONIC TICKET RECEIPT & ITINERARY")
                            .FontSize(9).SemiBold().FontColor(DarkText);
                        brandCol.Item().Text("IATA Standard Passenger Receipt • Non-Transferable")
                            .FontSize(7).FontColor(MutedText);
                    });
                });

                // Right: PNR & Confirmation Box
                row.RelativeItem(2).AlignRight().Column(metaCol =>
                {
                    metaCol.Item().Background(PrimaryLight).Border(1).BorderColor(PrimaryBorder).CornerRadius(4)
                        .PaddingHorizontal(8).PaddingVertical(4).Row(pnrRow =>
                        {
                            pnrRow.AutoItem().Text("BOOKING REF: ").FontSize(8).SemiBold().FontColor(MutedText);
                            pnrRow.AutoItem().Text(model.BookingReference).FontSize(10).Bold().FontColor(PrimaryColor);
                        });

                    metaCol.Item().PaddingTop(3).Text($"Issue Date: {model.BookingDate:dd MMM yyyy HH:mm} UTC")
                        .FontSize(7.5f).FontColor(MutedText);

                    metaCol.Item().PaddingTop(1).Row(statRow =>
                    {
                        statRow.AutoItem().Text("STATUS: ").FontSize(7.5f).SemiBold().FontColor(MutedText);
                        statRow.AutoItem().Text("CONFIRMED / PAID")
                            .FontSize(7.5f).Bold().FontColor(Color.FromHex("#16a34a"));
                    });
                });
            });

            col.Item().LineHorizontal(1).LineColor(BorderColor);
        });
    }

    private static void BuildTicketContent(IContainer container, TicketPdfModel model)
    {
        container.PaddingTop(10).Column(col =>
        {
            // ----------------------------------------------------
            // 1. PASSENGER & TICKET IDENTIFICATION CARD
            // ----------------------------------------------------
            col.Item().Background(CardBg).Border(1).BorderColor(BorderColor).CornerRadius(6).Padding(10).Row(row =>
            {
                // Passenger Name
                row.RelativeItem(3).Column(c =>
                {
                    c.Item().Text("PASSENGER NAME").FontSize(7).SemiBold().FontColor(MutedText);
                    c.Item().Text(model.PassengerName.ToUpper()).FontSize(11).Bold().FontColor(DarkText);
                    c.Item().Text($"Type: {model.PassengerType ?? "Adult"} • Meal: {model.MealPreference ?? "Standard"}").FontSize(7.5f).FontColor(MutedText);
                });

                // Ticket Number
                row.RelativeItem(3).Column(c =>
                {
                    c.Item().Text("ELECTRONIC TICKET NUMBER").FontSize(7).SemiBold().FontColor(MutedText);
                    c.Item().Text(model.TicketNumber).FontSize(11).Bold().FontColor(PrimaryColor);
                    c.Item().Text($"Issuing Carrier: {model.AirlineIata} ({model.AirlineName})").FontSize(7.5f).FontColor(MutedText);
                });

                // Passport / Travel Doc
                row.RelativeItem(2.5f).Column(c =>
                {
                    c.Item().Text("PASSPORT / TRAVEL DOC").FontSize(7).SemiBold().FontColor(MutedText);
                    c.Item().Text(string.IsNullOrWhiteSpace(model.PassportNumber) ? "ON FILE / VERIFIED" : model.PassportNumber)
                        .FontSize(9).Bold().FontColor(DarkText);
                    c.Item().Text($"Nationality / Country: {model.PassportCountry ?? "BD"}").FontSize(7.5f).FontColor(MutedText);
                });

                // Frequent Flyer / Class
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().Text("CABIN CLASS").FontSize(7).SemiBold().FontColor(MutedText);
                    c.Item().Text(model.SeatClass.ToUpper()).FontSize(9).Bold().FontColor(PrimaryColor);
                    c.Item().Text($"Seat: {model.SeatNumber ?? "Assigned at Check-in"}").FontSize(7.5f).FontColor(DarkText);
                });
            });

            // ----------------------------------------------------
            // 2. FLIGHT ITINERARY BANNER (VISUAL ROUTE)
            // ----------------------------------------------------
            col.Item().PaddingTop(12).Background(Colors.White).Border(1).BorderColor(BorderColor).CornerRadius(6).Column(itCol =>
            {
                // Route header strip
                itCol.Item().Background(PrimaryColor).PaddingHorizontal(12).PaddingVertical(6).Row(r =>
                {
                    r.RelativeItem().Text($"FLIGHT {model.FlightNumber} • {(model.AircraftModel ?? "Boeing 777-300ER")} • OPERATED BY {model.AirlineName.ToUpper()}")
                        .FontSize(8.5f).Bold().FontColor(Colors.White);

                    r.AutoItem().Text("NON-STOP SCHEDULED FLIGHT")
                        .FontSize(7.5f).SemiBold().FontColor(PrimaryLight);
                });

                // Route Visual Body
                itCol.Item().Padding(12).Row(routeRow =>
                {
                    // Origin Airport
                    routeRow.RelativeItem(3).Column(c =>
                    {
                        c.Item().Text("DEPARTURE").FontSize(7.5f).SemiBold().FontColor(MutedText);
                        c.Item().Text(model.OriginIata).FontSize(24).Bold().FontColor(PrimaryColor);
                        c.Item().Text(string.IsNullOrWhiteSpace(model.OriginAirportName) ? $"{model.OriginCity} Airport" : model.OriginAirportName)
                            .FontSize(9).Bold().FontColor(DarkText);
                        c.Item().Text($"{model.OriginCity}, {model.OriginCountry ?? "BD"}").FontSize(8).FontColor(MutedText);
                        c.Item().PaddingTop(4).Text($"Terminal: {model.OriginTerminal ?? "1"} • Gate: {model.Gate ?? "TBD"}")
                            .FontSize(8).SemiBold().FontColor(DarkText);
                        c.Item().PaddingTop(2).Text(model.DepartureTime.ToString("ddd, dd MMM yyyy • HH:mm") + " LT")
                            .FontSize(9).Bold().FontColor(PrimaryColor);
                    });

                    // Flight Path / Icon
                    routeRow.RelativeItem(2).AlignMiddle().AlignCenter().Column(c =>
                    {
                        var duration = model.ArrivalTime - model.DepartureTime;
                        var durText = duration.TotalHours >= 1 ? $"{(int)duration.TotalHours}h {duration.Minutes}m" : $"{duration.Minutes}m";

                        c.Item().AlignCenter().Text(durText).FontSize(8).SemiBold().FontColor(MutedText);
                        c.Item().AlignCenter().Text("──────── ✈ ────────").FontSize(11).Bold().FontColor(PrimaryColor);
                        c.Item().AlignCenter().Text("Confirmed Seat").FontSize(7.5f).FontColor(Color.FromHex("#16a34a"));
                    });

                    // Destination Airport
                    routeRow.RelativeItem(3).AlignRight().Column(c =>
                    {
                        c.Item().Text("ARRIVAL").FontSize(7.5f).SemiBold().FontColor(MutedText);
                        c.Item().Text(model.DestinationIata).FontSize(24).Bold().FontColor(PrimaryColor);
                        c.Item().Text(string.IsNullOrWhiteSpace(model.DestinationAirportName) ? $"{model.DestinationCity} Airport" : model.DestinationAirportName)
                            .FontSize(9).Bold().FontColor(DarkText);
                        c.Item().Text($"{model.DestinationCity}, {model.DestinationCountry ?? ""}")
                            .FontSize(8).FontColor(MutedText);
                        c.Item().PaddingTop(4).Text($"Terminal: {model.DestinationTerminal ?? "1"}")
                            .FontSize(8).SemiBold().FontColor(DarkText);
                        c.Item().PaddingTop(2).Text(model.ArrivalTime.ToString("ddd, dd MMM yyyy • HH:mm") + " LT")
                            .FontSize(9).Bold().FontColor(PrimaryColor);
                    });
                });

                // Flight specs footer bar
                itCol.Item().Background(CardBg).BorderTop(1).BorderColor(BorderColor).PaddingHorizontal(12).PaddingVertical(6).Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("BAGGAGE ALLOWANCE").FontSize(7).SemiBold().FontColor(MutedText);
                        c.Item().Text(model.BaggageAllowance ?? "1x 23kg Checked + 1x 7kg Cabin").FontSize(8).Bold().FontColor(DarkText);
                    });
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("BOARDING GATE OPENS").FontSize(7).SemiBold().FontColor(MutedText);
                        c.Item().Text(model.DepartureTime.AddMinutes(-45).ToString("HH:mm") + " LT (45 min prior)").FontSize(8).Bold().FontColor(DarkText);
                    });
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text("CHECK-IN CLOSES").FontSize(7).SemiBold().FontColor(MutedText);
                        c.Item().Text(model.DepartureTime.AddMinutes(-60).ToString("HH:mm") + " LT (60 min prior)").FontSize(8).Bold().FontColor(Color.FromHex("#b91c1c"));
                    });
                });
            });

            // ----------------------------------------------------
            // 3. FARE BREAKDOWN & PAYMENT RECEIPT + 2D QR CODE
            // ----------------------------------------------------
            col.Item().PaddingTop(12).Row(payRow =>
            {
                // Left: Financial Breakdown
                payRow.RelativeItem(3.2f).Background(CardBg).Border(1).BorderColor(BorderColor).CornerRadius(6).Padding(10).Column(fCol =>
                {
                    fCol.Item().Text("FARE BREAKDOWN & PAYMENT RECEIPT").FontSize(8).Bold().FontColor(PrimaryColor);
                    fCol.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(BorderColor);

                    fCol.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        // Base Fare
                        table.Cell().PaddingVertical(2).Text("Base Airfare").FontSize(8).FontColor(DarkText);
                        table.Cell().PaddingVertical(2).AlignRight().Text($"USD {model.BaseFare:F2}").FontSize(8).SemiBold().FontColor(DarkText);

                        // Taxes & Surcharges
                        table.Cell().PaddingVertical(2).Text("Airport Taxes & Govt. Security Surcharges").FontSize(8).FontColor(DarkText);
                        table.Cell().PaddingVertical(2).AlignRight().Text($"USD {model.TaxesAndFees:F2}").FontSize(8).SemiBold().FontColor(DarkText);

                        // Discount if any
                        if (model.DiscountAmount > 0)
                        {
                            table.Cell().PaddingVertical(2).Text("Promo Discount").FontSize(8).FontColor(Color.FromHex("#16a34a"));
                            table.Cell().PaddingVertical(2).AlignRight().Text($"-USD {model.DiscountAmount:F2}").FontSize(8).Bold().FontColor(Color.FromHex("#16a34a"));
                        }

                        // Total Line
                        table.Cell().ColumnSpan(2).PaddingVertical(3).LineHorizontal(1).LineColor(BorderColor);

                        table.Cell().PaddingVertical(2).Text("TOTAL FARE PAID").FontSize(9).Bold().FontColor(DarkText);
                        table.Cell().PaddingVertical(2).AlignRight().Text($"USD {model.TotalAmount:F2}").FontSize(11).Bold().FontColor(PrimaryColor);
                    });

                    fCol.Item().PaddingTop(6).Background(Colors.White).Border(0.5f).BorderColor(BorderColor).CornerRadius(4).Padding(6).Row(pmRow =>
                    {
                        pmRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PAYMENT METHOD").FontSize(7).SemiBold().FontColor(MutedText);
                            c.Item().Text(string.IsNullOrWhiteSpace(model.PaymentMethod) ? "Stripe Payment Gateway" : model.PaymentMethod)
                                .FontSize(7.5f).Bold().FontColor(DarkText);
                        });
                        pmRow.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text("TRANSACTION REF / INTENT").FontSize(7).SemiBold().FontColor(MutedText);
                            var txn = string.IsNullOrWhiteSpace(model.TransactionReference) ? $"TXN-{model.BookingReference}" : model.TransactionReference;
                            if (txn.Length > 24) txn = txn[..24] + "...";
                            c.Item().Text(txn).FontSize(7.5f).FontColor(DarkText);
                        });
                    });
                });

                payRow.ConstantItem(12);

                // Right: 2D Barcode & Verification Stub
                payRow.RelativeItem(1.8f).Background(CardBg).Border(1).BorderColor(BorderColor).CornerRadius(6).Padding(8).Column(qrCol =>
                {
                    qrCol.Item().AlignCenter().Text("BOARDING & AIRPORT PASS").FontSize(7.5f).Bold().FontColor(DarkText);
                    qrCol.Item().AlignCenter().Text("Present at Immigration & Security").FontSize(6.5f).FontColor(MutedText);

                    if (model.QrCodeBytes != null && model.QrCodeBytes.Length > 0)
                    {
                        qrCol.Item().PaddingTop(6).AlignCenter().Height(85).Width(85).Image(model.QrCodeBytes);
                    }
                    else
                    {
                        qrCol.Item().PaddingTop(15).AlignCenter().Height(70).Text("[QR Code Ready]").FontSize(8).FontColor(MutedText);
                    }

                    qrCol.Item().PaddingTop(4).AlignCenter().Text($"PNR: {model.BookingReference} • {model.TicketNumber}")
                        .FontSize(6.5f).SemiBold().FontColor(MutedText);
                    qrCol.Item().AlignCenter().Text("Authorized Electronic Document")
                        .FontSize(6).FontColor(Color.FromHex("#16a34a"));
                });
            });

            // ----------------------------------------------------
            // 4. IMPORTANT PASSENGER NOTICE & CARRIAGE CONDITIONS
            // ----------------------------------------------------
            col.Item().PaddingTop(12).Background(PrimaryLight).Border(1).BorderColor(PrimaryBorder).CornerRadius(6).Padding(8).Column(notCol =>
            {
                notCol.Item().Row(r =>
                {
                    r.AutoItem().Text("IMPORTANT TRAVEL NOTICE & IATA CONDITIONS").FontSize(7.5f).Bold().FontColor(PrimaryColor);
                });

                notCol.Item().PaddingTop(3).Table(t =>
                {
                    t.ColumnsDefinition(cd =>
                    {
                        cd.RelativeColumn();
                        cd.RelativeColumn();
                    });

                    t.Cell().PaddingRight(8).Column(c =>
                    {
                        c.Item().Text("• Check-in Counters: Open 3 hours prior and close strictly 60 minutes before departure. Passengers arriving late will be offloaded.").FontSize(6.5f).FontColor(DarkText);
                        c.Item().Text("• Passports & Visas: Passports must have at least 6 months validity from departure date. Valid travel visa is passenger's responsibility.").FontSize(6.5f).FontColor(DarkText);
                    });

                    t.Cell().Column(c =>
                    {
                        c.Item().Text("• Baggage Security: Power banks, e-cigarettes and spare lithium batteries must remain in carry-on cabin baggage only.").FontSize(6.5f).FontColor(DarkText);
                        c.Item().Text("• Flight Irregularities: Carriage hereunder is subject to the Warsaw / Montreal Convention. Keep this receipt for tax and immigration purposes.").FontSize(6.5f).FontColor(DarkText);
                    });
                });
            });
        });
    }

    private static void BuildTicketFooter(IContainer container, TicketPdfModel model)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(BorderColor);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text($"Customer Care: +1 (800) 555-0199 • support@airsystem.com • https://airsystem.example.com")
                    .FontSize(7).FontColor(MutedText);

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                    text.Span($" • Ref: {model.BookingReference}");
                });
            });
        });
    }

    // ==========================================
    // MODERN BOARDING PASS (A5 LANDSCAPE)
    // ==========================================

    private static void BuildBoardingPass(IContainer container, BoardingPassModel model)
    {
        container.Border(1.5f).BorderColor(PrimaryColor).CornerRadius(8).Column(rootCol =>
        {
            // Top Header Ribbon
            rootCol.Item().Background(PrimaryColor).PaddingHorizontal(14).PaddingVertical(8).Row(r =>
            {
                r.RelativeItem().Row(lr =>
                {
                    if (model.AirlineLogoBytes != null && model.AirlineLogoBytes.Length > 0)
                    {
                        lr.ConstantItem(26).Height(26).AlignMiddle().Image(model.AirlineLogoBytes);
                        lr.ConstantItem(8);
                    }
                    lr.RelativeItem().AlignMiddle().Text(string.IsNullOrWhiteSpace(model.AirlineName) ? "AirSystem" : model.AirlineName)
                        .FontSize(14).Bold().FontColor(Colors.White);
                });

                r.ConstantItem(120).AlignRight().AlignMiddle().Text("BOARDING PASS")
                    .FontSize(12).Bold().FontColor(PrimaryLight);

                r.ConstantItem(120).AlignRight().AlignMiddle().Background(Color.FromHex("#14532d")).CornerRadius(4).PaddingHorizontal(6).PaddingVertical(2)
                    .Text($"{model.SeatClass.ToUpper()} CLASS").FontSize(8.5f).Bold().FontColor(Colors.White);
            });

            // Split into Main Pass (70%) and Agent Stub (30%)
            rootCol.Item().Row(row =>
            {
                // ---------------- MAIN PASS (70%) ----------------
                row.RelativeItem(7).Padding(12).Column(c =>
                {
                    // Row 1: Passenger Name & Flight Info
                    c.Item().Row(r1 =>
                    {
                        r1.RelativeItem(3).Column(pc =>
                        {
                            pc.Item().Text("NAME OF PASSENGER").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            pc.Item().Text(model.PassengerName.ToUpper()).FontSize(12).Bold().FontColor(DarkText);
                        });

                        r1.RelativeItem(2).Column(pc =>
                        {
                            pc.Item().Text("FLIGHT NUMBER").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            pc.Item().Text(model.FlightNumber).FontSize(12).Bold().FontColor(PrimaryColor);
                        });

                        r1.RelativeItem(2).Column(pc =>
                        {
                            pc.Item().Text("DATE").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            pc.Item().Text(model.DepartureTime.ToString("dd MMM yyyy")).FontSize(10).Bold().FontColor(DarkText);
                        });

                        r1.RelativeItem(2).Column(pc =>
                        {
                            pc.Item().Text("PNR / REF").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            pc.Item().Text(model.BookingReference).FontSize(10).Bold().FontColor(PrimaryColor);
                        });
                    });

                    // Row 2: Origin to Destination Visual
                    c.Item().PaddingTop(8).Background(CardBg).Border(0.5f).BorderColor(BorderColor).CornerRadius(6).Padding(8).Row(r2 =>
                    {
                        r2.RelativeItem(3).Column(oc =>
                        {
                            oc.Item().Text("FROM").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            oc.Item().Text(model.OriginIata).FontSize(20).Bold().FontColor(PrimaryColor);
                            oc.Item().Text(model.OriginCity).FontSize(8).Bold().FontColor(DarkText);
                            oc.Item().Text($"Dep: {model.DepartureTime:HH:mm} LT").FontSize(7.5f).FontColor(MutedText);
                        });

                        r2.RelativeItem(2).AlignMiddle().AlignCenter().Column(ac =>
                        {
                            ac.Item().AlignCenter().Text("✈ NON-STOP").FontSize(7).SemiBold().FontColor(PrimaryColor);
                            ac.Item().AlignCenter().Text("──────────").FontSize(8).FontColor(BorderColor);
                            ac.Item().AlignCenter().Text(model.AircraftModel ?? "Boeing 777").FontSize(6.5f).FontColor(MutedText);
                        });

                        r2.RelativeItem(3).AlignRight().Column(dc =>
                        {
                            dc.Item().Text("TO").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            dc.Item().Text(model.DestinationIata).FontSize(20).Bold().FontColor(PrimaryColor);
                            dc.Item().Text(model.DestinationCity).FontSize(8).Bold().FontColor(DarkText);
                            dc.Item().Text($"Arr: {model.ArrivalTime:HH:mm} LT").FontSize(7.5f).FontColor(MutedText);
                        });
                    });

                    // Row 3: Boarding Essentials Callout Grid
                    c.Item().PaddingTop(8).Row(r3 =>
                    {
                        // Gate
                        r3.RelativeItem().Background(CardBg).Border(1).BorderColor(BorderColor).CornerRadius(4).Padding(6).Column(gc =>
                        {
                            gc.Item().Text("GATE").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            gc.Item().Text(string.IsNullOrWhiteSpace(model.Gate) ? "A12" : model.Gate).FontSize(14).Bold().FontColor(DarkText);
                            gc.Item().Text($"Terminal: {model.Terminal}").FontSize(6.5f).FontColor(MutedText);
                        });

                        r3.ConstantItem(8);

                        // Boarding Time
                        r3.RelativeItem().Background(Color.FromHex("#fef2f2")).Border(1).BorderColor(Color.FromHex("#fecaca")).CornerRadius(4).Padding(6).Column(bc =>
                        {
                            bc.Item().Text("BOARDING TIME").FontSize(6.5f).SemiBold().FontColor(Color.FromHex("#991b1b"));
                            bc.Item().Text(model.BoardingTime.ToString("HH:mm")).FontSize(14).Bold().FontColor(Color.FromHex("#b91c1c"));
                            bc.Item().Text("Gate Closes 20m Prior").FontSize(6.5f).FontColor(Color.FromHex("#991b1b"));
                        });

                        r3.ConstantItem(8);

                        // Zone
                        r3.RelativeItem().Background(CardBg).Border(1).BorderColor(BorderColor).CornerRadius(4).Padding(6).Column(zc =>
                        {
                            zc.Item().Text("BOARDING GROUP").FontSize(6.5f).SemiBold().FontColor(MutedText);
                            zc.Item().Text(model.BoardingGroup ?? "ZONE 2").FontSize(14).Bold().FontColor(DarkText);
                            zc.Item().Text("Follow Zone Calls").FontSize(6.5f).FontColor(MutedText);
                        });

                        r3.ConstantItem(8);

                        // Seat
                        r3.RelativeItem().Background(PrimaryLight).Border(1.5f).BorderColor(PrimaryColor).CornerRadius(4).Padding(6).Column(sc =>
                        {
                            sc.Item().Text("SEAT NUMBER").FontSize(6.5f).SemiBold().FontColor(PrimaryColor);
                            sc.Item().Text(model.SeatNumber ?? "TBD").FontSize(16).Bold().FontColor(PrimaryColor);
                            sc.Item().Text(model.SeatClass).FontSize(6.5f).FontColor(PrimaryColor);
                        });
                    });

                    // Row 4: Baggage & Notice
                    c.Item().PaddingTop(6).Row(r4 =>
                    {
                        r4.RelativeItem().Text($"Baggage: {model.BaggageAllowance ?? "1x 23kg Checked + 7kg Cabin"} • Present with Passport at Boarding Gate")
                            .FontSize(6.5f).FontColor(MutedText);
                    });
                });

                // ---------------- PERFORATED LINE ----------------
                row.ConstantItem(1).Background(BorderColor);

                // ---------------- AGENT STUB (30%) ----------------
                row.RelativeItem(3).Background(CardBg).Padding(10).Column(stubCol =>
                {
                    stubCol.Item().AlignCenter().Text("AGENT COUPON / STUB").FontSize(7.5f).Bold().FontColor(PrimaryColor);
                    stubCol.Item().AlignCenter().Text("Tear off at Boarding Gate").FontSize(6).FontColor(MutedText);
                    stubCol.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(BorderColor);

                    stubCol.Item().PaddingTop(4).Column(sc =>
                    {
                        sc.Item().Text("PASSENGER").FontSize(6).SemiBold().FontColor(MutedText);
                        sc.Item().Text(model.PassengerName.ToUpper()).FontSize(8.5f).Bold().FontColor(DarkText);
                    });

                    stubCol.Item().PaddingTop(4).Row(sr =>
                    {
                        sr.RelativeItem().Column(sc =>
                        {
                            sc.Item().Text("FLIGHT").FontSize(6).SemiBold().FontColor(MutedText);
                            sc.Item().Text(model.FlightNumber).FontSize(9).Bold().FontColor(PrimaryColor);
                        });
                        sr.RelativeItem().Column(sc =>
                        {
                            sc.Item().Text("SEAT").FontSize(6).SemiBold().FontColor(MutedText);
                            sc.Item().Text(model.SeatNumber ?? "TBD").FontSize(11).Bold().FontColor(PrimaryColor);
                        });
                    });

                    stubCol.Item().PaddingTop(3).Row(sr2 =>
                    {
                        sr2.RelativeItem().Column(sc =>
                        {
                            sc.Item().Text("FROM / TO").FontSize(6).SemiBold().FontColor(MutedText);
                            sc.Item().Text($"{model.OriginIata} → {model.DestinationIata}").FontSize(8).Bold().FontColor(DarkText);
                        });
                        sr2.RelativeItem().Column(sc =>
                        {
                            sc.Item().Text("GATE").FontSize(6).SemiBold().FontColor(MutedText);
                            sc.Item().Text(string.IsNullOrWhiteSpace(model.Gate) ? "A12" : model.Gate).FontSize(8.5f).Bold().FontColor(DarkText);
                        });
                    });

                    stubCol.Item().PaddingTop(3).Column(sc =>
                    {
                        sc.Item().Text("BOARDING TIME").FontSize(6).SemiBold().FontColor(MutedText);
                        sc.Item().Text(model.BoardingTime.ToString("HH:mm")).FontSize(9).Bold().FontColor(Color.FromHex("#b91c1c"));
                    });

                    // Barcode Scanner QR
                    if (model.QrCodeBytes != null && model.QrCodeBytes.Length > 0)
                    {
                        stubCol.Item().PaddingTop(6).AlignCenter().Height(55).Width(55).Image(model.QrCodeBytes);
                    }

                    stubCol.Item().PaddingTop(2).AlignCenter().Text(model.TicketNumber)
                        .FontSize(5.5f).FontColor(MutedText);
                });
            });
        });
    }
}
