using AirplaneSystem.Domain.Entities.Cms;
namespace AirplaneSystem.Application.Repositories;

public interface IPaymentGatewaySettingRepository : IRepository<PaymentGatewaySetting>
{
    Task<PaymentGatewaySetting?> GetByProviderAsync(string provider, CancellationToken ct = default);
    //Task<List<PaymentGatewaySetting>> GetAllAsync(CancellationToken ct = default);
}