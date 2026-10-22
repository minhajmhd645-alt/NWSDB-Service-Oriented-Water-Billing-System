using System.Collections.Generic;
using System.Threading.Tasks;
using NWSDB.API.DTOs.Payments;

namespace NWSDB.API.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string accountNumber);
        Task<PaymentDto?> GetPaymentAsync(int paymentId);
        Task<PaymentDto> CreatePaymentAsync(PaymentRequestDto paymentRequest);
    }
}
