using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NWSDB.Web.Models;

namespace NWSDB.Web.Services
{
    public interface INwsdbApiClient
    {
        Task<CustomerSummaryModel?> GetCustomerSummaryAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<CustomerModel?> GetCustomerAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<BillModel>> GetBillsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<BillModel>> GetUnpaidBillsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<BillModel?> GetBillAsync(int billId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PaymentModel>> GetPaymentHistoryAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<PaymentModel?> GetPaymentAsync(int paymentId, CancellationToken cancellationToken = default);
        Task<(bool Success, PaymentModel? Payment, string ErrorMessage)> MakePaymentAsync(PaymentRequestModel request, CancellationToken cancellationToken = default);
        Task<IEnumerable<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default);
        Task<string> GetNextAccountNumberAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, CustomerModel? Customer, string ErrorMessage, int StatusCode)> CreateCustomerAsync(CreateCustomerRequestModel request, CancellationToken cancellationToken = default);
        Task<(bool Success, CustomerModel? Customer, string ErrorMessage, int StatusCode)> UpdateCustomerAsync(string accountNumber, UpdateCustomerRequestModel request, CancellationToken cancellationToken = default);
        Task<IEnumerable<BillModel>> GetAllBillsAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, BillModel? Bill, string ErrorMessage, int StatusCode)> CreateBillAsync(CreateBillRequestModel request, CancellationToken cancellationToken = default);
        Task<(bool Success, BillModel? Bill, string ErrorMessage, int StatusCode)> UpdateBillAsync(int billId, UpdateBillRequestModel request, CancellationToken cancellationToken = default);
        Task<IEnumerable<PaymentModel>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);
        Task<AdminStatsModel?> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    }
}
