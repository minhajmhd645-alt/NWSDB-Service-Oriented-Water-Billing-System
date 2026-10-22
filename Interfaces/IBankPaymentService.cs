using Bank.API.DTOs.Payments;

namespace Bank.API.Interfaces
{
    /// <summary>
    /// Contract for the bank payment processing service.
    /// </summary>
    public interface IBankPaymentService
    {
        /// <summary>
        /// Validates and processes a payment request against a demo bank account.
        /// </summary>
        /// <param name="request">Payment details including account number, amount, and references.</param>
        /// <returns>A <see cref="PaymentResultDto"/> with the transaction outcome.</returns>
        /// <exception cref="ArgumentException">Thrown when validation fails (bad account, inactive, insufficient balance, etc.).</exception>
        Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto request);

        /// <summary>
        /// Retrieves a transaction by its unique bank transaction ID.
        /// </summary>
        Task<TransactionDto?> GetTransactionByIdAsync(string transactionId);

        /// <summary>
        /// Retrieves a transaction by the external reference supplied by the calling system.
        /// </summary>
        Task<TransactionDto?> GetTransactionByExternalReferenceAsync(string externalReference);
    }
}
