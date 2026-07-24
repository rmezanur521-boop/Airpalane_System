using AirplaneSystem.Application.Repositories;
using AirplaneSystem.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;

namespace AirplaneSystem.Infrastructure.Persistence.Repositories
{
    public class SmtpSettingRepository : Repository<SmtpSettings>, ISmtpSettingRepository
    {
        private readonly AppDbContext _context;

        public SmtpSettingRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<SmtpSettings> GetSingletonAsync(CancellationToken ct = default)
        {
            var settings = await _context.SmtpSettings.FirstOrDefaultAsync(ct);
            if (settings == null)
            {
                settings = new SmtpSettings();
                await _context.SmtpSettings.AddAsync(settings, ct);
                await _context.SaveChangesAsync(ct);
            }
            return settings;
        }
    }
}