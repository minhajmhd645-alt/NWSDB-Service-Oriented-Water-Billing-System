using System.Threading.Tasks;
using NWSDB.API.DTOs.Payments;

namespace NWSDB.API.Interfaces
{
    /// <summary>
    /// Abstraction over the Bank.API HTTP client.
    /// Allows PaymentService to be tested without real HTTP calls.
    /// </summary>
    public interface IBankApiClient
    {
        /// <summary>
        /// Sends a payment request to Bank.API and returns the result.
        /// Throws <see cref="InvalidOperationException"/> if the bank rejects the payment.
        /// Throws <see cref="HttpRequestException"/> on network/HTTP errors.
        /// </summary>
        Task<BankPaymentResult> ProcessPaymentAsync(BankPaymentRequest request);
    }
}
