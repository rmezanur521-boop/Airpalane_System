using AirplaneSystem.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AirplaneSystem.Application.BackgroundServices;

public class BookingExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpiryService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public BookingExpiryService(IServiceScopeFactory scopeFactory, ILogger<BookingExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingExpiryService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredBookingsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BookingExpiryService");
            }
            await Task.Delay(_interval, stoppingToken);
        }
        _logger.LogInformation("BookingExpiryService stopped.");
    }

    private async Task ProcessExpiredBookingsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await bookingService.ExpireHeldBookingsAsync(ct);
    }
}
