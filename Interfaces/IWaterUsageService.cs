using System.Collections.Generic;
using System.Threading.Tasks;
using NWSDB.API.DTOs.WaterUsage;

namespace NWSDB.API.Interfaces
{
    public interface IWaterUsageService
    {
        Task<IEnumerable<WaterUsageDto>> GetUsageHistoryAsync(string accountNumber);
        Task<WaterUsageDto?> GetCurrentUsageAsync(string accountNumber);
    }
}
