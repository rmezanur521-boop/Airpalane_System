using AirplaneSystem.Domain.Entities.Flights;
using AirplaneSystem.Domain.Entities.Payments;
using AirplaneSystem.Domain.Entities.Users;
using AirplaneSystem.Domain.Entities.Booking;
using AirplaneSystem.Domain.Entities.Tickets;
using AirplaneSystem.Domain.Enums;
using AirplaneSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AirplaneSystem.Domain.Entities.Cms;
namespace AirplaneSystem.Infrastructure.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        await context.Database.EnsureCreatedAsync();

        // 👇 CMS Singleton Rows (Navbar/Footer/Homepage) — একই কারণে Early Return-এর আগে রাখা হলো
        await SeedCmsSingletonsAsync(context, logger);
        await SeedCmsContentAsync(context, logger);

        if (await context.Airports.AnyAsync())
        {
            // Ensure flight schedules remain fresh and in the future
            if (await context.Flights.AnyAsync())
            {
                var minDep = await context.Flights.MinAsync(f => (DateTime?)f.DepartureTime);
                if (minDep.HasValue && minDep.Value.Date < DateTime.UtcNow.Date)
                {
                    var dayDiff = (int)(DateTime.UtcNow.Date - minDep.Value.Date).TotalDays;
                    if (dayDiff > 0)
                    {
                        await context.Database.ExecuteSqlInterpolatedAsync(
                            $"UPDATE Flights SET DepartureTime = DATEADD(day, {dayDiff}, DepartureTime), ArrivalTime = DATEADD(day, {dayDiff}, ArrivalTime)");
                        logger.LogInformation("Shifted flight schedules forward by {Days} days to ensure active flights.", dayDiff);
                    }
                }
            }
            await SeedRealisticDemoDataAsync(context, logger);
            return;
        }

        logger.LogInformation("Seeding database...");

        await SeedAirportsAsync(context);
        await SeedAirlinesAsync(context);
        await SeedAircraftsAsync(context);
        await SeedRoutesAsync(context);
        await SeedFlightsAsync(context);
        await SeedUsersAsync(context);
        await SeedPromoCodesAsync(context);
        await SeedRealisticDemoDataAsync(context, logger);

        logger.LogInformation("Database seeding completed.");
    }
    private static async Task SeedCmsSingletonsAsync(AppDbContext context, ILogger logger)
    {
        if (!await context.NavbarSettings.AnyAsync())
        {
            context.NavbarSettings.Add(new NavbarSetting { CompanyName = "AirplaneSystem" });
            logger.LogInformation("Seeded default NavbarSetting");
        }

        if (!await context.FooterSettings.AnyAsync())
        {
            context.FooterSettings.Add(new FooterSetting());
            logger.LogInformation("Seeded default FooterSetting");
        }

        if (!await context.HomepageSettings.AnyAsync())
        {
            context.HomepageSettings.Add(new HomepageSetting());
            logger.LogInformation("Seeded default HomepageSetting");
        }

        if (!await context.SmtpSettings.AnyAsync())
        {
            context.SmtpSettings.Add(new SmtpSettings());
            logger.LogInformation("Seeded default SmtpSetting");
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedCmsContentAsync(AppDbContext context, ILogger logger)
    {
        // 1. Update Navbar settings with logo and brand info if missing
        var navbar = await context.NavbarSettings.FirstOrDefaultAsync();
        if (navbar != null && string.IsNullOrEmpty(navbar.Logo))
        {
            navbar.CompanyName = "AirSystem";
            navbar.Logo = "/uploads/cms/navbar/de9f6b4d67264010a63dbeeb1d1d0b52.png";
            navbar.FaviconPath = "/uploads/cms/navbar/de9f6b4d67264010a63dbeeb1d1d0b52.png";
            navbar.WebsiteUrl = "https://airsystem.com";
            navbar.SupportPhone = "+1 (800) 555-0199";
            navbar.SupportEmail = "support@airsystem.com";
            navbar.ShowLogin = true;
            navbar.ShowSignup = true;
            navbar.ShowLanguage = true;
            navbar.ShowCurrency = true;
            navbar.AnnouncementEnabled = true;
            logger.LogInformation("Updated NavbarSetting with logo and brand info.");
        }

        // 2. Update Footer settings if missing
        var footer = await context.FooterSettings.FirstOrDefaultAsync();
        if (footer != null && string.IsNullOrEmpty(footer.About))
        {
            footer.About = "AirSystem is an international airline ticketing platform providing real-time flight bookings, flexible reservations, and 24/7 passenger assistance across the globe.";
            footer.Address = "123 Aviation Blvd, Suite 400, New York, NY 10001, USA";
            footer.Phone = "+1 (800) 555-0199";
            footer.Email = "support@airsystem.com";
            footer.Facebook = "https://facebook.com/airsystem";
            footer.Instagram = "https://instagram.com/airsystem";
            footer.Youtube = "https://youtube.com/airsystem";
            footer.LinkedIn = "https://linkedin.com/company/airsystem";
            footer.Twitter = "https://twitter.com/airsystem";
            footer.Copyright = "© 2026 AirSystem Inc. All rights reserved.";
            logger.LogInformation("Updated FooterSetting with company info.");
        }

        // 3. Hero Section
        if (!await context.HeroSections.AnyAsync())
        {
            context.HeroSections.Add(new HeroSection
            {
                Title = "Fly Beyond Limits With Unmatched Comfort",
                Subtitle = "Search hundreds of global routes and discover world-class flight deals in seconds.",
                BackgroundImage = "/uploads/cms/hero/fd58ed1e8dea4a3ea9fef3f3eca1af49.png",
                ButtonText = "Explore Destinations",
                ButtonLink = "/search",
                SearchBoxEnabled = true,
                OverlayOpacity = 0.45,
                Status = ContentStatus.Active,
                DisplayOrder = 1
            });
            logger.LogInformation("Seeded HeroSection.");
        }

        // 4. Special Offers
        if (!await context.SpecialOffers.AnyAsync())
        {
            var now = DateTime.UtcNow;
            context.SpecialOffers.AddRange(new List<SpecialOffer>
            {
                new()
                {
                    Title = "Summer Holiday Special: London to New York",
                    Description = "Enjoy exclusive discounts on transatlantic flights with complimentary seat selection and gourmet meals.",
                    OfferImage = "/uploads/cms/offers/025d3ae4b6734302a68a32e3bcc9372a.png",
                    Price = 499m,
                    PromoCode = "SUMMER25",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(4),
                    ButtonText = "Book Now",
                    ButtonLink = "/search?originIata=LHR&destinationIata=JFK&promoCode=SUMMER25",
                    Priority = 10,
                    Status = ContentStatus.Active,
                    Featured = true
                },
                new()
                {
                    Title = "Dubai Gateway: Luxury In The Skies",
                    Description = "Fly Emirates and experience luxurious cabins with up to 15% off business and economy fares.",
                    OfferImage = "/uploads/cms/offers/19701cc6bab442c5bb35c4272ed70a16.png",
                    Price = 389m,
                    PromoCode = "WELCOME10",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(5),
                    ButtonText = "Book Now",
                    ButtonLink = "/search?originIata=LHR&destinationIata=DXB&promoCode=WELCOME10",
                    Priority = 8,
                    Status = ContentStatus.Active,
                    Featured = true
                },
                new()
                {
                    Title = "Tokyo Cherry Blossom Express",
                    Description = "Experience the vibrant spring season in Tokyo with special low-fare roundtrip flights.",
                    OfferImage = "/uploads/cms/offers/f65d04cd74c0480e870ae2fb295697d9.png",
                    Price = 620m,
                    PromoCode = "AGENT15",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(6),
                    ButtonText = "Book Now",
                    ButtonLink = "/search?originIata=LAX&destinationIata=NRT&promoCode=AGENT15",
                    Priority = 7,
                    Status = ContentStatus.Active,
                    Featured = true
                },
                new()
                {
                    Title = "Romantic Paris Weekend Escape",
                    Description = "Fly direct to Paris Charles de Gaulle with complimentary extra baggage allowance.",
                    OfferImage = "/uploads/cms/offers/f82978acc38841b5879f50617a63a12d.png",
                    Price = 450m,
                    PromoCode = "WELCOME10",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(3),
                    ButtonText = "Book Now",
                    ButtonLink = "/search?originIata=JFK&destinationIata=CDG&promoCode=WELCOME10",
                    Priority = 6,
                    Status = ContentStatus.Active,
                    Featured = true
                }
            });
            logger.LogInformation("Seeded SpecialOffers.");
        }

        // 5. Popular Destinations
        if (!await context.PopularDestinations.AnyAsync())
        {
            context.PopularDestinations.AddRange(new List<PopularDestination>
            {
                new() { DestinationName = "Dubai", Country = "United Arab Emirates", Image = "/uploads/cms/destinations/062170846da6493da211223f1133fe6d.png", Description = "A dazzling metropolis of futuristic skyscrapers and luxury.", StartingPrice = 350m, ButtonLink = "/search?originIata=LHR&destinationIata=DXB", Featured = true, DisplayOrder = 1, Status = ContentStatus.Active },
                new() { DestinationName = "London", Country = "United Kingdom", Image = "/uploads/cms/destinations/36b4b487bec34b8ba80499e3ae440972.png", Description = "Iconic historic landmarks, rich culture, and royal charm.", StartingPrice = 420m, ButtonLink = "/search?originIata=JFK&destinationIata=LHR", Featured = true, DisplayOrder = 2, Status = ContentStatus.Active },
                new() { DestinationName = "Paris", Country = "France", Image = "/uploads/cms/destinations/3b72986814ca4fe8b728f5482f6acf4e.png", Description = "The city of light, world-famous art, fashion, and gastronomy.", StartingPrice = 390m, ButtonLink = "/search?originIata=JFK&destinationIata=CDG", Featured = true, DisplayOrder = 3, Status = ContentStatus.Active },
                new() { DestinationName = "Tokyo", Country = "Japan", Image = "/uploads/cms/destinations/4b2171de09734e63af144a8df626dd1b.png", Description = "A dynamic fusion of ancient traditions and cutting-edge tech.", StartingPrice = 580m, ButtonLink = "/search?originIata=LAX&destinationIata=NRT", Featured = true, DisplayOrder = 4, Status = ContentStatus.Active },
                new() { DestinationName = "New York", Country = "United States", Image = "/uploads/cms/destinations/527599dce4b747138e4ef57d15368d14.png", Description = "The vibrant heart of culture, entertainment, and commerce.", StartingPrice = 280m, ButtonLink = "/search?originIata=LHR&destinationIata=JFK", Featured = true, DisplayOrder = 5, Status = ContentStatus.Active },
                new() { DestinationName = "Singapore", Country = "Singapore", Image = "/uploads/cms/destinations/614be064500a4744a616348c487c8673.png", Description = "A futuristic garden city with world-class hospitality.", StartingPrice = 410m, ButtonLink = "/search?originIata=DXB&destinationIata=SIN", Featured = true, DisplayOrder = 6, Status = ContentStatus.Active },
                new() { DestinationName = "Rome", Country = "Italy", Image = "/uploads/cms/destinations/6a7ae301253342ee9a0add393dc11d95.png", Description = "Eternal history, Colosseum, and world-renowned Italian cuisine.", StartingPrice = 370m, ButtonLink = "/search?originIata=JFK&destinationIata=FCO", Featured = false, DisplayOrder = 7, Status = ContentStatus.Active },
                new() { DestinationName = "Sydney", Country = "Australia", Image = "/uploads/cms/destinations/89b8834ec122454e97eaaa1acfa4f328.png", Description = "Stunning harbor views, iconic Opera House, and sunny beaches.", StartingPrice = 650m, ButtonLink = "/search?originIata=SIN&destinationIata=SYD", Featured = false, DisplayOrder = 8, Status = ContentStatus.Active },
                new() { DestinationName = "Amsterdam", Country = "Netherlands", Image = "/uploads/cms/destinations/b9896e024cc845b29139b3d56903c762.png", Description = "Picturesque canals, historical museums, and biking culture.", StartingPrice = 360m, ButtonLink = "/search?originIata=JFK&destinationIata=AMS", Featured = false, DisplayOrder = 9, Status = ContentStatus.Active },
                new() { DestinationName = "Cape Town", Country = "South Africa", Image = "/uploads/cms/destinations/f3b5ccb67d9c4875867ed357d2a9ff15.png", Description = "Breathtaking mountain scenery and scenic coastal beauty.", StartingPrice = 490m, ButtonLink = "/search?originIata=LHR&destinationIata=CPT", Featured = false, DisplayOrder = 10, Status = ContentStatus.Active }
            });
            logger.LogInformation("Seeded PopularDestinations.");
        }

        // 6. Why Choose Us
        if (!await context.WhyChooseUsItems.AnyAsync())
        {
            context.WhyChooseUsItems.AddRange(new List<WhyChooseUsItem>
            {
                new() { Title = "Best Price Guarantee", Description = "We match any competitor's flight fares with transparent pricing and zero hidden fees.", Icon = "wallet", IconColor = "#027f22", DisplayOrder = 1, Status = ContentStatus.Active },
                new() { Title = "Safe & Certified Travel", Description = "Flying with IATA-certified airlines adhering strictly to world-class safety protocols.", Icon = "shield", IconColor = "#16a34a", DisplayOrder = 2, Status = ContentStatus.Active },
                new() { Title = "24/7 Dedicated Support", Description = "Our dedicated travel advisors are always on standby to support you anytime, anywhere.", Icon = "headphones", IconColor = "#8b5cf6", DisplayOrder = 3, Status = ContentStatus.Active },
                new() { Title = "Flexible Rebooking & Fast Refunds", Description = "Easily modify booking schedules or receive fast refund processing directly to your account.", Icon = "refresh", IconColor = "#ea580c", DisplayOrder = 4, Status = ContentStatus.Active },
                new() { Title = "World-Class Flight Experience", Description = "Enjoy spacious legroom, culinary onboard dining, and high-speed Wi-Fi in the air.", Icon = "award", IconColor = "#eab308", DisplayOrder = 5, Status = ContentStatus.Active },
                new() { Title = "Global Network Coverage", Description = "Seamless connections to over 500+ destinations worldwide across major airline alliances.", Icon = "globe", IconColor = "#06b6d4", DisplayOrder = 6, Status = ContentStatus.Active }
            });
            logger.LogInformation("Seeded WhyChooseUsItems.");
        }

        // 7. Fleet Items
        if (!await context.FleetItems.AnyAsync())
        {
            context.FleetItems.AddRange(new List<FleetItem>
            {
                new() { AircraftName = "Boeing 777-300ER", Manufacturer = "Boeing", Image = "/uploads/cms/fleet/0c287d60c19c4fdba5f1e6ecee68721c.png", SeatCapacity = 396, Range = "13,650 km", Description = "Flagship long-haul airliner with spacious twin-aisle cabins and ultra-quiet engines.", DisplayOrder = 1, Status = ContentStatus.Active },
                new() { AircraftName = "Airbus A350-900", Manufacturer = "Airbus", Image = "/uploads/cms/fleet/0c30b85ea4ad444cb5caf68fccac2129.png", SeatCapacity = 325, Range = "15,000 km", Description = "State-of-the-art carbon composite airframe with optimal cabin air pressure and ambient LED lighting.", DisplayOrder = 2, Status = ContentStatus.Active },
                new() { AircraftName = "Boeing 787-9 Dreamliner", Manufacturer = "Boeing", Image = "/uploads/cms/fleet/3c930eb10b8f4b74bfd1e3dc1ae9b1d8.png", SeatCapacity = 290, Range = "14,140 km", Description = "Revolutionary fuel efficiency with electrochromic dimmable windows and turbulence dampening technology.", DisplayOrder = 3, Status = ContentStatus.Active },
                new() { AircraftName = "Airbus A380-800", Manufacturer = "Airbus", Image = "/uploads/cms/fleet/6017264d28694bcdae8c59c364b2c40d.png", SeatCapacity = 516, Range = "15,200 km", Description = "The world's largest double-deck passenger aircraft with unmatched comfort and onboard lounge.", DisplayOrder = 4, Status = ContentStatus.Active },
                new() { AircraftName = "Airbus A321neo", Manufacturer = "Airbus", Image = "/uploads/cms/fleet/7c6f88291b514b868208e828697d9d27.png", SeatCapacity = 206, Range = "7,400 km", Description = "Single-aisle jet equipped with CFM LEAP engines delivering quiet, comfortable regional & medium-haul flights.", DisplayOrder = 5, Status = ContentStatus.Active },
                new() { AircraftName = "Boeing 737 MAX 9", Manufacturer = "Boeing", Image = "/uploads/cms/fleet/ee8c0c9ecad949949a62f174e74e4d91.png", SeatCapacity = 178, Range = "6,570 km", Description = "Modern short-to-medium range aircraft with Boeing Sky Interior and advanced aerodynamic winglets.", DisplayOrder = 6, Status = ContentStatus.Active }
            });
            logger.LogInformation("Seeded FleetItems.");
        }

        // 8. Travel Services
        if (!await context.TravelServices.AnyAsync())
        {
            context.TravelServices.AddRange(new List<TravelService>
            {
                new() { ServiceName = "Airport Transfer", Description = "Pre-book private luxury cars or express airport shuttles waiting for you at the terminal.", Icon = "car", Image = "/uploads/cms/services/36e45c0210e84640bbdf0eeb92eaece7.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 1, Status = ContentStatus.Active },
                new() { ServiceName = "Baggage Protection", Description = "Enjoy priority baggage handling, extra weight allowance, and comprehensive luggage tracking.", Icon = "luggage", Image = "/uploads/cms/services/37cbf05d1fe74dc3beb92a293d563ac1.jpeg", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 2, Status = ContentStatus.Active },
                new() { ServiceName = "Comprehensive Travel Insurance", Description = "Protect your international trip with emergency medical coverage and instant flight delay protection.", Icon = "umbrella", Image = "/uploads/cms/services/42e819e96e4d4afabe33d0e6905db587.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 3, Status = ContentStatus.Active },
                new() { ServiceName = "VIP Airport Lounge Access", Description = "Unwind before your flight with complimentary gourmet buffet, bar, private showers, and quiet suites.", Icon = "coffee", Image = "/uploads/cms/services/524eae0bcbbe494f96c7327391158a67.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 4, Status = ContentStatus.Active },
                new() { ServiceName = "Luxury Hotel Reservations", Description = "Book partner 5-star hotels and luxury resorts worldwide with up to 20% discount on flight pairings.", Icon = "building", Image = "/uploads/cms/services/60c855a5ca914eadaf6932f55c1078ae.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 5, Status = ContentStatus.Active },
                new() { ServiceName = "Seat Upgrade & Selection", Description = "Select extra-legroom seats or upgrade instantly to lie-flat Business Class suites.", Icon = "armchair", Image = "/uploads/cms/services/907c4e9b935e43f3919893295f4200bf.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 6, Status = ContentStatus.Active },
                new() { ServiceName = "Special Inflight Meals", Description = "Pre-order gourmet halal, vegan, kosher, and kid-friendly meals prepared by award-winning chefs.", Icon = "utensils", Image = "/uploads/cms/services/f9289f77b96447829964beafc8c32055.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 7, Status = ContentStatus.Active },
                new() { ServiceName = "Fast Track Security", Description = "Breeze through airport customs and priority security lanes with dedicated express passes.", Icon = "fast", Image = "/uploads/cms/services/fad0cacae2ea424098e11ff471a1cff1.png", RedirectUrl = "/search", IsExternal = false, DisplayOrder = 8, Status = ContentStatus.Active }
            });
            logger.LogInformation("Seeded TravelServices.");
        }

        // 9. Announcement Bars
        if (!await context.AnnouncementBars.AnyAsync())
        {
            var now = DateTime.UtcNow;
            context.AnnouncementBars.AddRange(new List<AnnouncementBar>
            {
                new()
                {
                    Title = "✈️ Exclusive Winter Sale: Get up to 25% off international flights with code SUMMER25!",
                    BackgroundColor = "#027f22",
                    TextColor = "#ffffff",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(3),
                    Priority = 10,
                    Status = ContentStatus.Active
                },
                new()
                {
                    Title = "🎉 Free date changes & instant refunds on all Flex flight tickets booked this month.",
                    BackgroundColor = "#0f172a",
                    TextColor = "#f8fafc",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddMonths(4),
                    Priority = 5,
                    Status = ContentStatus.Active
                }
            });
            logger.LogInformation("Seeded AnnouncementBars.");
        }

        await context.SaveChangesAsync();
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

    private static async Task SeedRealisticDemoDataAsync(AppDbContext context, ILogger logger)
    {
        // 1. Update Airlines Logos & Contact Info & Gallery
        var airlines = await context.Airlines.ToListAsync();
        foreach (var airline in airlines)
        {
            var updated = false;
            if (string.IsNullOrEmpty(airline.LogoUrl))
            {
                airline.LogoUrl = airline.IataCode switch
                {
                    "AA" => "https://images.kiwi.com/airlines/64/AA.png",
                    "BA" => "https://images.kiwi.com/airlines/64/BA.png",
                    "EK" => "https://images.kiwi.com/airlines/64/EK.png",
                    "DL" => "https://images.kiwi.com/airlines/64/DL.png",
                    "UA" => "https://images.kiwi.com/airlines/64/UA.png",
                    "SQ" => "https://images.kiwi.com/airlines/64/SQ.png",
                    "QF" => "https://images.kiwi.com/airlines/64/QF.png",
                    "LH" => "https://images.kiwi.com/airlines/64/LH.png",
                    _ => $"https://images.kiwi.com/airlines/64/{airline.IataCode}.png"
                };
                updated = true;
            }
            if (string.IsNullOrEmpty(airline.ContactEmail))
            {
                airline.ContactEmail = $"support@{airline.IataCode.ToLowerInvariant()}airlines.com";
                updated = true;
            }
            if (string.IsNullOrEmpty(airline.ContactPhone))
            {
                airline.ContactPhone = "+1 800-555-0199";
                updated = true;
            }
            if (updated) context.Airlines.Update(airline);
        }
        await context.SaveChangesAsync();

        if (!await context.Set<AirlineImage>().AnyAsync())
        {
            var fleetImages = new[]
            {
                "/uploads/cms/fleet/0c287d60c19c4fdba5f1e6ecee68721c.png",
                "/uploads/cms/fleet/0c30b85ea4ad444cb5caf68fccac2129.png",
                "/uploads/cms/fleet/3c930eb10b8f4b74bfd1e3dc1ae9b1d8.png",
                "/uploads/cms/fleet/6017264d28694bcdae8c59c364b2c40d.png",
                "/uploads/cms/fleet/7c6f88291b514b868208e828697d9d27.png",
                "/uploads/cms/fleet/ee8c0c9ecad949949a62f174e74e4d91.png"
            };
            var imgIdx = 0;
            foreach (var al in airlines)
            {
                context.Set<AirlineImage>().Add(new AirlineImage
                {
                    AirlineId = al.Id,
                    ImageUrl = fleetImages[imgIdx % fleetImages.Length],
                    IsPrimary = true,
                    SortOrder = 1
                });
                imgIdx++;
            }
            await context.SaveChangesAsync();
        }

        // 2. Seed Payment Gateway Settings
        if (!await context.PaymentGatewaySettings.AnyAsync())
        {
            context.PaymentGatewaySettings.AddRange(
                new PaymentGatewaySetting
                {
                    Provider = "Stripe",
                    IsEnabled = true,
                    PublicKey = "pk_test_51MzDemoKeyAirlineSystem99xL",
                    SecretKey = "sk_test_demoSecretKeyEncrypted123",
                    WebhookSecret = "whsec_demoWebhookSecret456",
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentGatewaySetting
                {
                    Provider = "Bkash",
                    IsEnabled = true,
                    PublicKey = "bkash_app_key_live_0192",
                    SecretKey = "bkash_secret_demoEncrypted",
                    UpdatedAt = DateTime.UtcNow
                },
                new PaymentGatewaySetting
                {
                    Provider = "Nagad",
                    IsEnabled = true,
                    PublicKey = "nagad_merchant_id_008",
                    SecretKey = "nagad_secret_demoEncrypted",
                    UpdatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded PaymentGatewaySettings (Stripe, Bkash, Nagad).");
        }

        // 3. Seed Additional Realistic Passengers & Passports
        var existingEmails = await context.Users.Select(u => u.Email).ToListAsync();
        var newUsers = new List<User>();

        void AddUserIfMissing(string email, string first, string last, string phone, string nat, DateOnly dob)
        {
            if (existingEmails.Contains(email)) return;
            newUsers.Add(new User
            {
                FirstName = first,
                LastName = last,
                Email = email,
                PhoneNumber = phone,
                Nationality = nat,
                DateOfBirth = dob,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@1234", 12),
                Role = UserRole.Passenger,
                IsEmailVerified = true,
                IsActive = true
            });
        }

        AddUserIfMissing("sarah.jenkins@gmail.com", "Sarah", "Jenkins", "+14155552671", "US", new DateOnly(1994, 5, 12));
        AddUserIfMissing("rahim.ahmed@yahoo.com", "Rahim", "Ahmed", "+8801711223344", "BD", new DateOnly(1988, 11, 23));
        AddUserIfMissing("david.miller@outlook.com", "David", "Miller", "+447911123456", "GB", new DateOnly(1991, 8, 4));
        AddUserIfMissing("elena.rostova@travel.com", "Elena", "Rostova", "+491512345678", "DE", new DateOnly(1996, 2, 17));

        if (newUsers.Count > 0)
        {
            await context.Users.AddRangeAsync(newUsers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} additional passenger users.", newUsers.Count);
        }

        var users = await context.Users.Include(u => u.PassportInfo).ToListAsync();
        foreach (var u in users)
        {
            if (u.PassportInfo == null && u.Role == UserRole.Passenger)
            {
                var pNum = $"A{Math.Abs(u.Id.GetHashCode()) % 90000000 + 10000000}";
                context.PassportInfos.Add(new PassportInfo
                {
                    UserId = u.Id,
                    PassportNumber = pNum,
                    IssuingCountry = string.IsNullOrEmpty(u.Nationality) ? "US" : u.Nationality,
                    IssuedDate = new DateOnly(2022, 1, 15),
                    ExpiryDate = new DateOnly(2032, 1, 14)
                });
            }
        }
        await context.SaveChangesAsync();

        // 4. Seed Realistic Bookings, Tickets, Payments & Refunds (if bookings <= 1)
        if (await context.Bookings.CountAsync() <= 1)
        {
            logger.LogInformation("Seeding realistic flight bookings, tickets, and payments...");

            // Clean up any initial orphan booking
            var oldBookings = await context.Bookings.ToListAsync();
            if (oldBookings.Count > 0)
            {
                context.Bookings.RemoveRange(oldBookings);
                await context.SaveChangesAsync();
            }

            var passengerUser = users.FirstOrDefault(u => u.Email == "passenger@airsystem.com") ?? users.First(u => u.Role == UserRole.Passenger);
            var samihaUser = users.FirstOrDefault(u => u.Email == "samiha@gmail.com") ?? passengerUser;
            var sarahUser = users.FirstOrDefault(u => u.Email == "sarah.jenkins@gmail.com") ?? passengerUser;
            var rahimUser = users.FirstOrDefault(u => u.Email == "rahim.ahmed@yahoo.com") ?? passengerUser;
            var davidUser = users.FirstOrDefault(u => u.Email == "david.miller@outlook.com") ?? passengerUser;
            var elenaUser = users.FirstOrDefault(u => u.Email == "elena.rostova@travel.com") ?? passengerUser;
            var adminUser = users.First(u => u.Role == UserRole.Admin);

            var flights = await context.Flights
                .Include(f => f.Route).ThenInclude(r => r.OriginAirport)
                .Include(f => f.Route).ThenInclude(r => r.DestinationAirport)
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();

            if (flights.Count == 0) return;

            var seats = await context.Seats.ToListAsync();
            var today = DateTime.UtcNow;

            Flight FindFlight(string origin, string dest, int minDaysAhead) =>
                flights.FirstOrDefault(f => f.Route.OriginAirport.IataCode == origin && f.Route.DestinationAirport.IataCode == dest && f.DepartureTime >= today.AddDays(minDaysAhead))
                ?? flights.FirstOrDefault(f => f.Route.OriginAirport.IataCode == origin && f.Route.DestinationAirport.IataCode == dest)
                ?? flights.First();

            var bookingsData = new[]
            {
                // passenger@airsystem.com (John Passenger)
                new {
                    User = passengerUser,
                    Origin = "JFK", Dest = "LHR", DaysAhead = 2,
                    Ref = "SW782K", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Economy, Meal = "Standard",
                    PaxFirst = "John", PaxLast = "Passenger", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Succeeded,
                    TxId = "ch_3O7xqB2eZvKYlo2C19824", DaysAgoCreated = 1,
                    IsCheckedIn = false, Refunded = false
                },
                new {
                    User = passengerUser,
                    Origin = "DXB", Dest = "SIN", DaysAhead = 5,
                    Ref = "SW419D", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Business, Meal = "Halal",
                    PaxFirst = "John", PaxLast = "Passenger", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.MobileBanking, PayStatus = PaymentStatus.Succeeded,
                    TxId = "TRX-BK78192348", DaysAgoCreated = 2,
                    IsCheckedIn = true, Refunded = false
                },
                new {
                    User = passengerUser,
                    Origin = "JFK", Dest = "LAX", DaysAhead = 7,
                    Ref = "SW105L", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Economy, Meal = "Standard",
                    PaxFirst = "John", PaxLast = "Passenger", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Succeeded,
                    TxId = "ch_3P18xQ2eZvKYlo2C88912", DaysAgoCreated = 3,
                    IsCheckedIn = false, Refunded = false
                },
                new {
                    User = passengerUser,
                    Origin = "LHR", Dest = "JFK", DaysAhead = -2,
                    Ref = "SW882L", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Economy, Meal = "Vegetarian",
                    PaxFirst = "John", PaxLast = "Passenger", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Succeeded,
                    TxId = "ch_3M92xB2eZvKYlo2C77103", DaysAgoCreated = 6,
                    IsCheckedIn = true, Refunded = false
                },
                new {
                    User = passengerUser,
                    Origin = "LAX", Dest = "NRT", DaysAhead = 10,
                    Ref = "SW331N", Status = BookingStatus.PendingPayment,
                    SeatClass = SeatClass.Economy, Meal = "Standard",
                    PaxFirst = "John", PaxLast = "Passenger", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.BankTransfer, PayStatus = PaymentStatus.PendingApproval,
                    TxId = "TXN-BNK9921045", DaysAgoCreated = 0,
                    IsCheckedIn = false, Refunded = false
                },

                // samiha@gmail.com
                new {
                    User = samihaUser,
                    Origin = "FRA", Dest = "BOM", DaysAhead = 3,
                    Ref = "SW512B", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Economy, Meal = "Halal",
                    PaxFirst = "Md", PaxLast = "Samiha", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.MobileBanking, PayStatus = PaymentStatus.Succeeded,
                    TxId = "TRX-NG8821943", DaysAgoCreated = 2,
                    IsCheckedIn = false, Refunded = false
                },
                new {
                    User = samihaUser,
                    Origin = "SYD", Dest = "SIN", DaysAhead = 8,
                    Ref = "SW901S", Status = BookingStatus.Refunded,
                    SeatClass = SeatClass.Economy, Meal = "Standard",
                    PaxFirst = "Md", PaxLast = "Samiha", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Refunded,
                    TxId = "ch_3Q41xB2eZvKYlo2C55829", DaysAgoCreated = 5,
                    IsCheckedIn = false, Refunded = true
                },

                // sarah.jenkins@gmail.com
                new {
                    User = sarahUser,
                    Origin = "JFK", Dest = "ORD", DaysAhead = 1,
                    Ref = "SW724O", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Economy, Meal = "Vegetarian",
                    PaxFirst = "Sarah", PaxLast = "Jenkins", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Succeeded,
                    TxId = "ch_3N88xB2eZvKYlo2C44910", DaysAgoCreated = 0,
                    IsCheckedIn = true, Refunded = false
                },
                new {
                    User = sarahUser,
                    Origin = "LHR", Dest = "DXB", DaysAhead = 6,
                    Ref = "SW229D", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.First, Meal = "Standard",
                    PaxFirst = "Sarah", PaxLast = "Jenkins", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Succeeded,
                    TxId = "ch_3K77xB2eZvKYlo2C33819", DaysAgoCreated = 4,
                    IsCheckedIn = false, Refunded = false
                },

                // rahim.ahmed@yahoo.com
                new {
                    User = rahimUser,
                    Origin = "DXB", Dest = "SIN", DaysAhead = 4,
                    Ref = "SW612S", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Economy, Meal = "Halal",
                    PaxFirst = "Rahim", PaxLast = "Ahmed", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.MobileBanking, PayStatus = PaymentStatus.Succeeded,
                    TxId = "TRX-BK9912048", DaysAgoCreated = 1,
                    IsCheckedIn = false, Refunded = false
                },

                // david.miller@outlook.com
                new {
                    User = davidUser,
                    Origin = "JFK", Dest = "LHR", DaysAhead = 3,
                    Ref = "SW390L", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Business, Meal = "Standard",
                    PaxFirst = "David", PaxLast = "Miller", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Succeeded,
                    TxId = "ch_3R22xB2eZvKYlo2C99014", DaysAgoCreated = 3,
                    IsCheckedIn = false, Refunded = false
                },

                // elena.rostova@travel.com
                new {
                    User = elenaUser,
                    Origin = "ICN", Dest = "LAX", DaysAhead = 5,
                    Ref = "SW551L", Status = BookingStatus.Confirmed,
                    SeatClass = SeatClass.Business, Meal = "Standard",
                    PaxFirst = "Elena", PaxLast = "Rostova", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.BankTransfer, PayStatus = PaymentStatus.Succeeded,
                    TxId = "TXN-BNK551029", DaysAgoCreated = 4,
                    IsCheckedIn = false, Refunded = false
                },
                new {
                    User = elenaUser,
                    Origin = "LAX", Dest = "NRT", DaysAhead = 9,
                    Ref = "SW712N", Status = BookingStatus.Cancelled,
                    SeatClass = SeatClass.Economy, Meal = "Standard",
                    PaxFirst = "Elena", PaxLast = "Rostova", PaxType = PassengerType.Adult,
                    Method = PaymentMethod.Stripe, PayStatus = PaymentStatus.Cancelled,
                    TxId = "ch_3T99xB2eZvKYlo2C11234", DaysAgoCreated = 5,
                    IsCheckedIn = false, Refunded = false
                }
            };

            var ticketIndex = 90410;
            foreach (var data in bookingsData)
            {
                var flight = FindFlight(data.Origin, data.Dest, data.DaysAhead);
                var basePrice = flight.GetBasePriceForClass(data.SeatClass);
                var tax = Math.Round(basePrice * (flight.TaxPercentage / 100), 2);
                var fee = flight.AirportFee;
                var totalAmount = basePrice + tax + fee;

                var createdDate = today.AddDays(-data.DaysAgoCreated).AddHours(-3);

                var booking = new Booking
                {
                    BookingReference = data.Ref,
                    UserId = data.User.Id,
                    Status = data.Status,
                    TripType = TripType.OneWay,
                    TotalAmount = totalAmount,
                    CurrencyCode = "USD",
                    ConfirmedAt = data.Status == BookingStatus.Confirmed ? createdDate.AddMinutes(5) : null,
                    CancelledAt = data.Status == BookingStatus.Cancelled || data.Status == BookingStatus.Refunded ? createdDate.AddHours(2) : null,
                    CancellationReason = data.Status == BookingStatus.Cancelled || data.Status == BookingStatus.Refunded ? "Customer requested cancellation" : null,
                    CreatedAt = createdDate,
                    UpdatedAt = createdDate.AddMinutes(5)
                };
                await context.Bookings.AddAsync(booking);

                var segment = new BookingSegment
                {
                    BookingId = booking.Id,
                    FlightId = flight.Id,
                    SegmentOrder = 1,
                    SeatClass = data.SeatClass,
                    BaseFare = basePrice,
                    Taxes = tax,
                    Fees = fee,
                    SegmentTotal = totalAmount,
                    CreatedAt = createdDate
                };
                await context.BookingSegments.AddAsync(segment);

                var assignedSeat = seats.FirstOrDefault(s => s.FlightId == flight.Id && s.SeatClass == data.SeatClass);

                var passenger = new BookingPassenger
                {
                    BookingId = booking.Id,
                    FirstName = data.PaxFirst,
                    LastName = data.PaxLast,
                    PassengerType = data.PaxType,
                    DateOfBirth = data.User.DateOfBirth,
                    PassportNumber = $"A{Math.Abs(data.User.Id.GetHashCode()) % 90000000 + 10000000}",
                    PassportExpiry = new DateOnly(2032, 1, 14),
                    PassportCountry = string.IsNullOrEmpty(data.User.Nationality) ? "US" : data.User.Nationality,
                    SeatId = assignedSeat?.Id,
                    MealPreference = data.Meal,
                    CreatedAt = createdDate
                };
                await context.BookingPassengers.AddAsync(passenger);

                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Method = data.Method,
                    StripePaymentIntentId = data.Method == PaymentMethod.Stripe ? data.TxId : null,
                    ReferenceNumber = data.Method != PaymentMethod.Stripe ? data.TxId : null,
                    Amount = totalAmount,
                    CurrencyCode = "USD",
                    Status = data.PayStatus,
                    PaidAt = data.PayStatus == PaymentStatus.Succeeded || data.PayStatus == PaymentStatus.Refunded ? createdDate.AddMinutes(5) : null,
                    ApprovedAt = data.Method == PaymentMethod.BankTransfer && data.PayStatus == PaymentStatus.Succeeded ? createdDate.AddMinutes(15) : null,
                    ApprovedBy = data.Method == PaymentMethod.BankTransfer && data.PayStatus == PaymentStatus.Succeeded ? adminUser.Id : null,
                    CreatedAt = createdDate
                };
                await context.Payments.AddAsync(payment);

                if (data.Refunded)
                {
                    await context.Refunds.AddAsync(new Refund
                    {
                        BookingId = booking.Id,
                        PaymentId = payment.Id,
                        Amount = totalAmount,
                        Status = RefundStatus.Processed,
                        Reason = "Passenger requested schedule cancellation",
                        RequestedAt = createdDate.AddHours(1),
                        ProcessedAt = createdDate.AddHours(2),
                        CreatedAt = createdDate.AddHours(1)
                    });
                }

                if (data.Status == BookingStatus.Confirmed)
                {
                    ticketIndex++;
                    var ticketNum = $"ET-2026-{ticketIndex}";
                    var qrPayload = $"{{\"ticketNumber\":\"{ticketNum}\",\"flight\":\"{flight.FlightNumber}\",\"passenger\":\"{passenger.FullName}\",\"route\":\"{flight.Route.OriginAirport.IataCode}-{flight.Route.DestinationAirport.IataCode}\"}}";

                    await context.Tickets.AddAsync(new Ticket
                    {
                        BookingId = booking.Id,
                        BookingPassengerId = passenger.Id,
                        BookingSegmentId = segment.Id,
                        TicketNumber = ticketNum,
                        IssuedAt = createdDate.AddMinutes(5),
                        QrCodeData = qrPayload,
                        IsCheckedIn = data.IsCheckedIn,
                        CheckedInAt = data.IsCheckedIn ? createdDate.AddHours(3) : null,
                        CreatedAt = createdDate.AddMinutes(5)
                    });
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Successfully seeded realistic flight bookings, tickets, payments, and refunds!");
        }
    }
}
