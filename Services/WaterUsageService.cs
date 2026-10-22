using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWSDB.API.Data;
using NWSDB.API.DTOs.WaterUsage;
using NWSDB.API.Interfaces;

namespace NWSDB.API.Services
{
    public class WaterUsageService : IWaterUsageService
    {
        private readonly NWSDBDbContext _context;

        public WaterUsageService(NWSDBDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WaterUsageDto>> GetUsageHistoryAsync(string accountNumber)
        {
            var usages = await _context.WaterUsages
                .Where(u => u.AccountNumber == accountNumber)
                .OrderByDescending(u => u.RecordedAt)
                .AsNoTracking()
                .ToListAsync();

            return usages.Select(u => new WaterUsageDto
            {
                ReadingMonth = u.ReadingMonth,
                PreviousReading = u.PreviousReading,
                CurrentReading = u.CurrentReading,
                UnitsConsumed = u.UnitsConsumed,
                RecordedDate = u.RecordedAt
            });
        }

        public async Task<WaterUsageDto?> GetCurrentUsageAsync(string accountNumber)
        {
            var u = await _context.WaterUsages
                .Where(x => x.AccountNumber == accountNumber)
                .OrderByDescending(x => x.RecordedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (u == null) return null;

            return new WaterUsageDto
            {
                ReadingMonth = u.ReadingMonth,
                PreviousReading = u.PreviousReading,
                CurrentReading = u.CurrentReading,
                UnitsConsumed = u.UnitsConsumed,
                RecordedDate = u.RecordedAt
            };
        }
    }
}
