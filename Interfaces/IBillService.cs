using System.Collections.Generic;
using System.Threading.Tasks;
using NWSDB.API.DTOs.Bills;

namespace NWSDB.API.Interfaces
{
    public interface IBillService
    {
        Task<IEnumerable<BillDto>> GetAllBillsAsync();
        Task<IEnumerable<BillDto>> GetBillsAsync(string accountNumber);
        Task<IEnumerable<BillDto>> GetUnpaidBillsAsync(string accountNumber);
        Task<BillDto?> GetBillAsync(int billId);
        Task<BillDto> CreateBillAsync(CreateBillDto dto);
        Task<BillDto?> UpdateBillAsync(int billId, UpdateBillDto dto);
    }
}
