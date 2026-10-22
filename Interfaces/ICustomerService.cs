using System.Collections.Generic;
using System.Threading.Tasks;
using NWSDB.API.DTOs.Customers;

namespace NWSDB.API.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerAsync(string accountNumber);
        Task<CustomerSummaryDto?> GetCustomerSummaryAsync(string accountNumber);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
        Task<CustomerDto?> UpdateCustomerAsync(string accountNumber, UpdateCustomerDto dto);
        Task<string> GetNextAccountNumberAsync();
    }
}
