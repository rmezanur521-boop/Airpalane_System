using AirplaneSystem.Application.Common.Interfaces;
using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Application.Services.Interfaces;
using AirplaneSystem.Infrastructure.ExternalServices.Cache;
using AirplaneSystem.Infrastructure.ExternalServices.Email;
using AirplaneSystem.Infrastructure.ExternalServices.Pdf;
using AirplaneSystem.Infrastructure.ExternalServices.QrCode;
using AirplaneSystem.Infrastructure.ExternalServices.Sms;
using AirplaneSystem.Infrastructure.ExternalServices.Stripe;
using AirplaneSystem.Infrastructure.Persistence;
using AirplaneSystem.Infrastructure.Persistence.Repositories;
using AirplaneSystem.Infrastructure.Security;
using AirplaneSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AirplaneSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                    .CommandTimeout(60)));
                    //.EnableRetryOnFailure(3)));

        // Repositories & Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IAirportRepository, AirportRepository>();
        services.AddScoped<IAirlineRepository, AirlineRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();

        // Security
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // Infrastructure services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // External services
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddScoped<IPdfService, QuestPdfService>();
        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<IPaymentService, StripePaymentService>();

        // Cache
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        return services;
    }
}
