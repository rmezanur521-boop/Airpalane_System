using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories;

public class AnnouncementBarRepository : Repository<AnnouncementBar>, IAnnouncementBarRepository
{
    public AnnouncementBarRepository(AppDbContext context) : base(context) { }
}