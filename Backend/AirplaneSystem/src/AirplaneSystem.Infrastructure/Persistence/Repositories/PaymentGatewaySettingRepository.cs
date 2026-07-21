using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class PaymentGatewaySettingRepository : Repository<PaymentGatewaySetting>, IPaymentGatewaySettingRepository
{
    public PaymentGatewaySettingRepository(AppDbContext context) : base(context) { }

    public async Task<PaymentGatewaySetting?> GetByProviderAsync(string provider, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(p => p.Provider == provider, ct);

}