using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class SpecialOfferRepository : Repository<SpecialOffer>, ISpecialOfferRepository
{
    public SpecialOfferRepository(AppDbContext context) : base(context) { }
}