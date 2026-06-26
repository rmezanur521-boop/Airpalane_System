using AirplaneSystem.Domain.Entities.Payments;

namespace AirplaneSystem.Application.Repositories;

public interface IPromoCodeRepository : IRepository<PromoCode>
{
    Task<PromoCode?> GetByCodeAsync(string code, CancellationToken ct = default);
}
