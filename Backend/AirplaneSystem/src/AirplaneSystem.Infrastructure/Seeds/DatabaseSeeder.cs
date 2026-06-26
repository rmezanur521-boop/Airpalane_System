using AirplaneSystem.Domain.Entities.Flights;
using AirplaneSystem.Domain.Entities.Payments;
using AirplaneSystem.Domain.Entities.Users;
using AirplaneSystem.Domain.Enums;
using AirplaneSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Infrastructure.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Airports.AnyAsync()) return;

        logger.LogInformation("Seeding database...");

        await SeedAirportsAsync(context);
        await SeedAirlinesAsync(context);
        await SeedAircraftsAsync(context);
        await SeedRoutesAsync(context);
        await SeedFlightsAsync(context);
        await SeedUsersAsync(context);
        await SeedPromoCodesAsync(context);

        logger.LogInformation("Database seeding completed.");
    }

    private static async Task SeedAirportsAsync(AppDbContext context)
    {
        var airports = new List<Airport>
        {
            new() { IataCode="JFK", IcaoCode="KJFK", Name="John F. Kennedy International", City="New York", Country="United States", CountryCode="US", Latitude=40.641766m, Longitude=-73.780968m, TimeZone="America/New_York", Terminal="T4" },
            new() { IataCode="LAX", IcaoCode="KLAX", Name="Los Angeles International", City="Los Angeles", Country="United States", CountryCode="US", Latitude=33.942791m, Longitude=-118.410042m, TimeZone="America/Los_Angeles", Terminal="B" },
            new() { IataCode="ORD", IcaoCode="KORD", Name="O'Hare International", City="Chicago", Country="United States", CountryCode="US", Latitude=41.974162m, Longitude=-87.907321m, TimeZone="America/Chicago", Terminal="1" },
            new() { IataCode="ATL", IcaoCode="KATL", Name="Hartsfield-Jackson Atlanta International", City="Atlanta", Country="United States", CountryCode="US", Latitude=33.640411m, Longitude=-84.426864m, TimeZone="America/New_York", Terminal="T" },
            new() { IataCode="DFW", IcaoCode="KDFW", Name="Dallas/Fort Worth International", City="Dallas", Country="United States", CountryCode="US", Latitude=32.897480m, Longitude=-97.040443m, TimeZone="America/Chicago", Terminal="A" },
            new() { IataCode="MIA", IcaoCode="KMIA", Name="Miami International", City="Miami", Country="United States", CountryCode="US", Latitude=25.795865m, Longitude=-80.287046m, TimeZone="America/New_York", Terminal="J" },
            new() { IataCode="SFO", IcaoCode="KSFO", Name="San Francisco International", City="San Francisco", Country="United States", CountryCode="US", Latitude=37.618972m, Longitude=-122.374889m, TimeZone="America/Los_Angeles", Terminal="3" },
            new() { IataCode="SEA", IcaoCode="KSEA", Name="Seattle-Tacoma International", City="Seattle", Country="United States", CountryCode="US", Latitude=47.449888m, Longitude=-122.313988m, TimeZone="America/Los_Angeles", Terminal="S" },
            new() { IataCode="BOS", IcaoCode="KBOS", Name="Logan International", City="Boston", Country="United States", CountryCode="US", Latitude=42.364347m, Longitude=-71.005181m, TimeZone="America/New_York", Terminal="E" },
            new() { IataCode="DEN", IcaoCode="KDEN", Name="Denver International", City="Denver", Country="United States", CountryCode="US", Latitude=39.856094m, Longitude=-104.673738m, TimeZone="America/Denver", Terminal="B" },
            new() { IataCode="LHR", IcaoCode="EGLL", Name="Heathrow Airport", City="London", Country="United Kingdom", CountryCode="GB", Latitude=51.477500m, Longitude=-0.461389m, TimeZone="Europe/London", Terminal="T5" },
            new() { IataCode="CDG", IcaoCode="LFPG", Name="Charles de Gaulle Airport", City="Paris", Country="France", CountryCode="FR", Latitude=49.012779m, Longitude=2.550000m, TimeZone="Europe/Paris", Terminal="2E" },
            new() { IataCode="FRA", IcaoCode="EDDF", Name="Frankfurt Airport", City="Frankfurt", Country="Germany", CountryCode="DE", Latitude=50.033333m, Longitude=8.570556m, TimeZone="Europe/Berlin", Terminal="1" },
            new() { IataCode="AMS", IcaoCode="EHAM", Name="Amsterdam Airport Schiphol", City="Amsterdam", Country="Netherlands", CountryCode="NL", Latitude=52.308601m, Longitude=4.763889m, TimeZone="Europe/Amsterdam", Terminal="D" },
            new() { IataCode="MAD", IcaoCode="LEMD", Name="Adolfo Suárez Madrid-Barajas", City="Madrid", Country="Spain", CountryCode="ES", Latitude=40.471926m, Longitude=-3.562624m, TimeZone="Europe/Madrid", Terminal="T4" },
            new() { IataCode="FCO", IcaoCode="LIRF", Name="Leonardo da Vinci International", City="Rome", Country="Italy", CountryCode="IT", Latitude=41.804475m, Longitude=12.250797m, TimeZone="Europe/Rome", Terminal="3" },
            new() { IataCode="DXB", IcaoCode="OMDB", Name="Dubai International Airport", City="Dubai", Country="United Arab Emirates", CountryCode="AE", Latitude=25.252778m, Longitude=55.364444m, TimeZone="Asia/Dubai", Terminal="T3" },
            new() { IataCode="SIN", IcaoCode="WSSS", Name="Singapore Changi Airport", City="Singapore", Country="Singapore", CountryCode="SG", Latitude=1.359167m, Longitude=103.989441m, TimeZone="Asia/Singapore", Terminal="T3" },
            new() { IataCode="NRT", IcaoCode="RJAA", Name="Narita International Airport", City="Tokyo", Country="Japan", CountryCode="JP", Latitude=35.764722m, Longitude=140.386389m, TimeZone="Asia/Tokyo", Terminal="T2" },
            new() { IataCode="HKG", IcaoCode="VHHH", Name="Hong Kong International Airport", City="Hong Kong", Country="Hong Kong", CountryCode="HK", Latitude=22.308900m, Longitude=113.914603m, TimeZone="Asia/Hong_Kong", Terminal="T1" },
            new() { IataCode="SYD", IcaoCode="YSSY", Name="Kingsford Smith International", City="Sydney", Country="Australia", CountryCode="AU", Latitude=-33.946110m, Longitude=151.177200m, TimeZone="Australia/Sydney", Terminal="T1" },
            new() { IataCode="ICN", IcaoCode="RKSI", Name="Incheon International Airport", City="Seoul", Country="South Korea", CountryCode="KR", Latitude=37.469012m, Longitude=126.450656m, TimeZone="Asia/Seoul", Terminal="T2" },
            new() { IataCode="PEK", IcaoCode="ZBAA", Name="Beijing Capital International", City="Beijing", Country="China", CountryCode="CN", Latitude=40.072498m, Longitude=116.588562m, TimeZone="Asia/Shanghai", Terminal="T3" },
            new() { IataCode="BOM", IcaoCode="VABB", Name="Chhatrapati Shivaji International", City="Mumbai", Country="India", CountryCode="IN", Latitude=19.088700m, Longitude=72.867919m, TimeZone="Asia/Kolkata", Terminal="T2" },
            new() { IataCode="DEL", IcaoCode="VIDP", Name="Indira Gandhi International", City="New Delhi", Country="India", CountryCode="IN", Latitude=28.566700m, Longitude=77.103104m, TimeZone="Asia/Kolkata", Terminal="T3" },
            new() { IataCode="GRU", IcaoCode="SBGR", Name="São Paulo/Guarulhos International", City="São Paulo", Country="Brazil", CountryCode="BR", Latitude=-23.432800m, Longitude=-46.469799m, TimeZone="America/Sao_Paulo", Terminal="T3" },
            new() { IataCode="YYZ", IcaoCode="CYYZ", Name="Toronto Pearson International", City="Toronto", Country="Canada", CountryCode="CA", Latitude=43.677223m, Longitude=-79.630556m, TimeZone="America/Toronto", Terminal="T1" },
            new() { IataCode="MEX", IcaoCode="MMMX", Name="Benito Juárez International", City="Mexico City", Country="Mexico", CountryCode="MX", Latitude=19.436303m, Longitude=-99.072098m, TimeZone="America/Mexico_City", Terminal="T2" },
            new() { IataCode="CPT", IcaoCode="FACT", Name="Cape Town International Airport", City="Cape Town", Country="South Africa", CountryCode="ZA", Latitude=-33.964806m, Longitude=18.601667m, TimeZone="Africa/Johannesburg", Terminal="D" },
            new() { IataCode="JNB", IcaoCode="FAJS", Name="O.R. Tambo International Airport", City="Johannesburg", Country="South Africa", CountryCode="ZA", Latitude=-26.133694m, Longitude=28.242317m, TimeZone="Africa/Johannesburg", Terminal="B" }
        };
        await context.Airports.AddRangeAsync(airports);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAirlinesAsync(AppDbContext context)
    {
        var airlines = new List<Airline>
        {
            new() { IataCode="AA", Name="American Airlines", Country="United States", ContactEmail="support@aa.com" },
            new() { IataCode="BA", Name="British Airways", Country="United Kingdom", ContactEmail="support@ba.com" },
            new() { IataCode="LH", Name="Lufthansa", Country="Germany", ContactEmail="support@lufthansa.com" },
            new() { IataCode="EK", Name="Emirates", Country="United Arab Emirates", ContactEmail="support@emirates.com" },
            new() { IataCode="SQ", Name="Singapore Airlines", Country="Singapore", ContactEmail="support@singaporeair.com" },
            new() { IataCode="UA", Name="United Airlines", Country="United States", ContactEmail="support@united.com" },
            new() { IataCode="DL", Name="Delta Air Lines", Country="United States", ContactEmail="support@delta.com" },
            new() { IataCode="QF", Name="Qantas Airways", Country="Australia", ContactEmail="support@qantas.com" }
        };
        await context.Airlines.AddRangeAsync(airlines);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAircraftsAsync(AppDbContext context)
    {
        var airlines = await context.Airlines.ToListAsync();
        var aa = airlines.First(a => a.IataCode == "AA");
        var ba = airlines.First(a => a.IataCode == "BA");
        var lh = airlines.First(a => a.IataCode == "LH");
        var ek = airlines.First(a => a.IataCode == "EK");
        var sq = airlines.First(a => a.IataCode == "SQ");
        var ua = airlines.First(a => a.IataCode == "UA");
        var dl = airlines.First(a => a.IataCode == "DL");
        var qf = airlines.First(a => a.IataCode == "QF");

        var aircrafts = new List<Aircraft>
        {
            new() { AirlineId=aa.Id, Model="Boeing 777-300ER", RegistrationNumber="N700AA", TotalSeats=396, EconomySeats=294, BusinessSeats=68, FirstClassSeats=34 },
            new() { AirlineId=aa.Id, Model="Boeing 737-800", RegistrationNumber="N800AA", TotalSeats=160, EconomySeats=126, BusinessSeats=34, FirstClassSeats=0 },
            new() { AirlineId=ba.Id, Model="Boeing 787-9", RegistrationNumber="G-ZBKA", TotalSeats=216, EconomySeats=154, BusinessSeats=42, FirstClassSeats=20 },
            new() { AirlineId=ba.Id, Model="Airbus A380-800", RegistrationNumber="G-XLEA", TotalSeats=469, EconomySeats=303, BusinessSeats=97, FirstClassSeats=69 },
            new() { AirlineId=lh.Id, Model="Airbus A350-900", RegistrationNumber="D-AIXA", TotalSeats=293, EconomySeats=225, BusinessSeats=48, FirstClassSeats=20 },
            new() { AirlineId=lh.Id, Model="Airbus A320neo", RegistrationNumber="D-AINB", TotalSeats=168, EconomySeats=150, BusinessSeats=18, FirstClassSeats=0 },
            new() { AirlineId=ek.Id, Model="Airbus A380-800", RegistrationNumber="A6-EDA", TotalSeats=519, EconomySeats=399, BusinessSeats=76, FirstClassSeats=44 },
            new() { AirlineId=ek.Id, Model="Boeing 777-300ER", RegistrationNumber="A6-EBJ", TotalSeats=364, EconomySeats=266, BusinessSeats=72, FirstClassSeats=26 },
            new() { AirlineId=sq.Id, Model="Airbus A350-900", RegistrationNumber="9V-SMA", TotalSeats=253, EconomySeats=187, BusinessSeats=42, FirstClassSeats=24 },
            new() { AirlineId=ua.Id, Model="Boeing 787-9", RegistrationNumber="N38479", TotalSeats=252, EconomySeats=186, BusinessSeats=48, FirstClassSeats=18 },
            new() { AirlineId=dl.Id, Model="Boeing 767-400", RegistrationNumber="N843MH", TotalSeats=226, EconomySeats=170, BusinessSeats=38, FirstClassSeats=18 },
            new() { AirlineId=qf.Id, Model="Airbus A380-800", RegistrationNumber="VH-OQA", TotalSeats=484, EconomySeats=332, BusinessSeats=72, FirstClassSeats=80 }
        };
        await context.Aircrafts.AddRangeAsync(aircrafts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRoutesAsync(AppDbContext context)
    {
        var airports = await context.Airports.ToDictionaryAsync(a => a.IataCode);

        Route R(string origin, string destination, int distKm, int avgMinutes) => new()
        {
            OriginAirportId = airports[origin].Id,
            DestinationAirportId = airports[destination].Id,
            DistanceKm = distKm,
            AverageFlightMinutes = avgMinutes
        };

        var routes = new List<Route>
        {
            R("JFK","LHR",5539,420), R("LHR","JFK",5539,440),
            R("JFK","CDG",5837,445), R("CDG","JFK",5837,480),
            R("LAX","NRT",8769,640), R("NRT","LAX",8769,600),
            R("JFK","DXB",11023,820), R("DXB","JFK",11023,840),
            R("LHR","DXB",5480,410), R("DXB","LHR",5480,430),
            R("DXB","SIN",5846,440), R("SIN","DXB",5846,450),
            R("SYD","SIN",6308,480), R("SIN","SYD",6308,490),
            R("FRA","BOM",6245,470), R("BOM","FRA",6245,490),
            R("ICN","LAX",9620,700), R("LAX","ICN",9620,720),
            R("JFK","ORD",1190,150), R("ORD","JFK",1190,145),
            R("LAX","JFK",3978,330), R("JFK","LAX",3978,340),
            R("LHR","FRA",622,90), R("FRA","LHR",622,95),
            R("SIN","HKG",2571,215), R("HKG","SIN",2571,220),
            R("YYZ","LHR",5728,435), R("LHR","YYZ",5728,440),
            R("GRU","JFK",7737,580), R("JFK","GRU",7737,595),
        };
        await context.Routes.AddRangeAsync(routes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFlightsAsync(AppDbContext context)
    {
        var airlines = await context.Airlines.ToDictionaryAsync(a => a.IataCode);
        var aircrafts = await context.Aircrafts.ToListAsync();
        var routes = await context.Routes
            .Include(r => r.OriginAirport)
            .Include(r => r.DestinationAirport)
            .ToListAsync();

        Route? GetRoute(string origin, string dest) =>
            routes.FirstOrDefault(r => r.OriginAirport.IataCode == origin && r.DestinationAirport.IataCode == dest);

        var today = DateTime.UtcNow.Date;
        var flights = new List<Flight>();

        void AddFlight(string number, string airlineIata, int aircraftIndex, string origin, string dest,
            DateTime dep, int durationMins, decimal econPrice, decimal bizPrice, decimal firstPrice)
        {
            var route = GetRoute(origin, dest);
            if (route == null) return;
            var airline = airlines[airlineIata];
            var aircraft = aircrafts.Where(a => a.AirlineId == airline.Id).ElementAtOrDefault(aircraftIndex)
                ?? aircrafts.First(a => a.AirlineId == airline.Id);

            flights.Add(new Flight
            {
                FlightNumber = number,
                AirlineId = airline.Id,
                AircraftId = aircraft.Id,
                RouteId = route.Id,
                DepartureTime = dep,
                ArrivalTime = dep.AddMinutes(durationMins),
                Status = FlightStatus.Scheduled,
                EconomyBasePrice = econPrice,
                BusinessBasePrice = bizPrice,
                FirstClassBasePrice = firstPrice,
                AirportFee = 45.00m,
                TaxPercentage = 12.5m,
                AvailableEconomySeats = aircraft.EconomySeats,
                AvailableBusinessSeats = aircraft.BusinessSeats,
                AvailableFirstClassSeats = aircraft.FirstClassSeats,
                GateNumber = "A" + (flights.Count % 20 + 1)
            });
        }

        // JFK → LHR daily for 30 days
        for (int i = 1; i <= 30; i++)
        {
            AddFlight($"AA{100+i}", "AA", 0, "JFK", "LHR", today.AddDays(i).AddHours(18), 420, 650m, 2200m, 5800m);
            AddFlight($"BA{200+i}", "BA", 0, "JFK", "LHR", today.AddDays(i).AddHours(22), 430, 680m, 2400m, 6200m);
        }

        // LHR → JFK
        for (int i = 1; i <= 30; i++)
            AddFlight($"AA{300+i}", "AA", 0, "LHR", "JFK", today.AddDays(i).AddHours(10), 440, 620m, 2100m, 5500m);

        // LAX → NRT
        for (int i = 1; i <= 20; i++)
            AddFlight($"UA{100+i}", "UA", 0, "LAX", "NRT", today.AddDays(i).AddHours(13), 640, 780m, 2800m, 0m);

        // DXB → SIN
        for (int i = 1; i <= 20; i++)
            AddFlight($"EK{100+i}", "EK", 0, "DXB", "SIN", today.AddDays(i).AddHours(2), 440, 420m, 1600m, 4200m);

        // SIN → DXB
        for (int i = 1; i <= 20; i++)
            AddFlight($"SQ{100+i}", "SQ", 0, "SIN", "DXB", today.AddDays(i).AddHours(23), 450, 390m, 1500m, 3900m);

        // LHR → DXB
        for (int i = 1; i <= 20; i++)
            AddFlight($"EK{200+i}", "EK", 0, "LHR", "DXB", today.AddDays(i).AddHours(21), 410, 480m, 1800m, 4800m);

        // SYD → SIN
        for (int i = 1; i <= 20; i++)
            AddFlight($"QF{100+i}", "QF", 0, "SYD", "SIN", today.AddDays(i).AddHours(6), 480, 350m, 1400m, 3600m);

        // FRA → BOM
        for (int i = 1; i <= 15; i++)
            AddFlight($"LH{100+i}", "LH", 0, "FRA", "BOM", today.AddDays(i).AddHours(14), 470, 520m, 1900m, 4500m);

        // ICN → LAX
        for (int i = 1; i <= 15; i++)
            AddFlight($"AA{500+i}", "AA", 0, "ICN", "LAX", today.AddDays(i).AddHours(17), 700, 680m, 2500m, 0m);

        // Domestic USA
        for (int i = 1; i <= 30; i++)
        {
            AddFlight($"AA{700+i}", "AA", 1, "JFK", "ORD", today.AddDays(i).AddHours(7), 150, 180m, 450m, 0m);
            AddFlight($"DL{100+i}", "DL", 0, "JFK", "LAX", today.AddDays(i).AddHours(8), 340, 220m, 550m, 0m);
        }

        await context.Flights.AddRangeAsync(flights);
        await context.SaveChangesAsync();

        // Seed seats for first 10 flights (for testing)
        var firstFlights = flights.Take(10).ToList();
        var seats = new List<Seat>();
        foreach (var flight in firstFlights)
        {
            var aircraft = aircrafts.First(a => a.Id == flight.AircraftId);
            // Economy rows 10-40
            for (int row = 10; row <= 10 + (aircraft.EconomySeats / 6) - 1 && row <= 50; row++)
                foreach (char col in "ABCDEF")
                    seats.Add(new Seat { FlightId = flight.Id, SeatNumber = $"{row}{col}", SeatClass = SeatClass.Economy, IsAvailable = true, IsWindowSeat = col == 'A' || col == 'F', IsAisleSeat = col == 'C' || col == 'D' });

            // Business rows 1-5
            for (int row = 1; row <= 5; row++)
                foreach (char col in "ABCD")
                    seats.Add(new Seat { FlightId = flight.Id, SeatNumber = $"{row}{col}", SeatClass = SeatClass.Business, IsAvailable = true, ExtraLegroom = true });

            // First Class rows 1-3
            if (aircraft.FirstClassSeats > 0)
                for (int row = 1; row <= 3; row++)
                    foreach (char col in "AB")
                        seats.Add(new Seat { FlightId = flight.Id, SeatNumber = $"F{row}{col}", SeatClass = SeatClass.First, IsAvailable = true, ExtraLegroom = true });
        }
        await context.Seats.AddRangeAsync(seats);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        var users = new List<User>
        {
            new()
            {
                FirstName = "System", LastName = "Admin",
                Email = "admin@airsystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234", 12),
                Role = UserRole.Admin, IsEmailVerified = true, IsActive = true,
                PhoneNumber = "+12125550001", DateOfBirth = new DateOnly(1985, 1, 1), Nationality = "US"
            },
            new()
            {
                FirstName = "Travel", LastName = "Agent",
                Email = "agent@airsystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234", 12),
                Role = UserRole.Agent, IsEmailVerified = true, IsActive = true,
                PhoneNumber = "+12125550002", DateOfBirth = new DateOnly(1990, 6, 15), Nationality = "US"
            },
            new()
            {
                FirstName = "John", LastName = "Passenger",
                Email = "passenger@airsystem.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234", 12),
                Role = UserRole.Passenger, IsEmailVerified = true, IsActive = true,
                PhoneNumber = "+12125550003", DateOfBirth = new DateOnly(1992, 3, 20), Nationality = "US"
            }
        };
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPromoCodesAsync(AppDbContext context)
    {
        var promos = new List<PromoCode>
        {
            new()
            {
                Code = "WELCOME10", DiscountPercentage = 10, MaxUses = 1000, TimesUsed = 0,
                ValidFrom = DateTime.UtcNow.AddDays(-1), ValidTo = DateTime.UtcNow.AddYears(1),
                MinimumAmount = 100, IsActive = true
            },
            new()
            {
                Code = "SUMMER25", DiscountAmount = 25, MaxUses = 500, TimesUsed = 0,
                ValidFrom = DateTime.UtcNow.AddDays(-1), ValidTo = DateTime.UtcNow.AddMonths(6),
                MinimumAmount = 200, IsActive = true
            },
            new()
            {
                Code = "AGENT15", DiscountPercentage = 15, MaxUses = int.MaxValue, TimesUsed = 0,
                ValidFrom = DateTime.UtcNow.AddDays(-1), ValidTo = DateTime.UtcNow.AddYears(2),
                MinimumAmount = 0, IsActive = true
            }
        };
        await context.PromoCodes.AddRangeAsync(promos);
        await context.SaveChangesAsync();
    }
}
