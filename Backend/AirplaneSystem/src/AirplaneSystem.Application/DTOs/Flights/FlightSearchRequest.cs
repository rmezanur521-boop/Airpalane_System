using AirplaneSystem.Domain.Enums;

namespace AirplaneSystem.Application.DTOs.Flights;

public class FlightSearchRequest
{
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
    public DateOnly? ReturnDate { get; set; }
    public PassengerCount Passengers { get; set; } = new();
    public SeatClass SeatClass { get; set; } = SeatClass.Economy;
    public int MaxStops { get; set; } = 2;
    public string SortBy { get; set; } = "price";
    public bool SortDescending { get; set; } = false;
    public decimal? MaxPrice { get; set; }
}

public class MultiCitySearchRequest
{
    public List<FlightLeg> Legs { get; set; } = new();
    public PassengerCount Passengers { get; set; } = new();
    public SeatClass SeatClass { get; set; } = SeatClass.Economy;
}

public class FlightLeg
{
    public string OriginIata { get; set; } = string.Empty;
    public string DestinationIata { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
}

public class PassengerCount
{
    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int Infants { get; set; } = 0;
    public int Total => Adults + Children + Infants;
}
