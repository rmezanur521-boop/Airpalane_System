using AirplaneSystem.Application.BackgroundServices;
using AirplaneSystem.Application.Mappings;
using AirplaneSystem.Application.Services.Implementations;
using AirplaneSystem.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AirplaneSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAdminSettingService, AdminSettingService>();

        services.AddHostedService<BookingExpiryService>();

        return services;
    }
}
