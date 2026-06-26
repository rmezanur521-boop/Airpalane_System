using AirplaneSystem.Domain.Common;
using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Domain.Entities.Booking;

public class BookingSegment : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid FlightId { get; set; }
    public int SegmentOrder { get; set; }
    public SeatClass SeatClass { get; set; }
    public decimal BaseFare { get; set; }
    public decimal Taxes { get; set; }
    public decimal Fees { get; set; }
    public decimal SegmentTotal { get; set; }

    public Booking Booking { get; set; } = null!;
    public Flights.Flight Flight { get; set; } = null!;
    public ICollection<Tickets.Ticket> Tickets { get; set; } = new List<Tickets.Ticket>();
}
