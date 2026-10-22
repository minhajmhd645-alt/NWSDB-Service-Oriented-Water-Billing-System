using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NWSDB.ThirdPartyClient.Models;

namespace NWSDB.ThirdPartyClient.Services
{
    public interface INwsdbApiClient
    {
        Task<ResponseDto<CustomerDto>?> GetCustomerAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<ResponseDto<CustomerSummaryDto>?> GetCustomerSummaryAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<ResponseDto<IEnumerable<BillDto>>?> GetBillsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<ResponseDto<IEnumerable<BillDto>>?> GetUnpaidBillsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<ResponseDto<WaterUsageDto>?> GetCurrentUsageAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<ResponseDto<IEnumerable<PaymentDto>>?> GetPaymentsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<ResponseDto<PaymentDto>?> ProcessPaymentAsync(PaymentRequestDto request, CancellationToken cancellationToken = default);
    }
}
