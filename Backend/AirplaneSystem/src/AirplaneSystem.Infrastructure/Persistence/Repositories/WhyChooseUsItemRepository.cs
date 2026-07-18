using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class WhyChooseUsItemRepository : Repository<WhyChooseUsItem>, IWhyChooseUsItemRepository
{
    public WhyChooseUsItemRepository(AppDbContext context) : base(context) { }
}