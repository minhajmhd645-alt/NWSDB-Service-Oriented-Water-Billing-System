namespace NWSDB.API.DTOs.Payments
{
    /// <summary>
    /// Request body sent to Bank.API POST /api/v1/bank/payments.
    /// Field names and types must match Bank.API's PaymentRequestDto exactly.
    /// </summary>
    public class BankPaymentRequest
    {
        /// <summary>Demo bank account number (e.g. DEMO-BANK-001).</summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>Bill reference for cross-system correlation (e.g. NWSDB-BILL-10001).</summary>
        public string BillReference { get; set; } = string.Empty;

        /// <summary>Payment amount in the specified currency.</summary>
        public decimal Amount { get; set; }

        /// <summary>Three-letter currency code. Defaults to LKR.</summary>
        public string Currency { get; set; } = "LKR";

        /// <summary>Optional human-readable description for the transaction.</summary>
        public string? Description { get; set; }

        /// <summary>Payment method. Allowed: DemoBankAccount, DemoCard.</summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Unique external reference supplied by NWSDB to correlate transactions
        /// across systems and prevent duplicates (e.g. NWSDB-PAY-A1B2C3D4).
        /// </summary>
        public string ExternalReference { get; set; } = string.Empty;
    }

    /// <summary>
    /// Successful payment result returned by Bank.API (unwrapped from ResponseDto&lt;T&gt;).
    /// </summary>
    public class BankPaymentResult
    {
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "LKR";
        public string Message { get; set; } = string.Empty;
        public string ProcessedAt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Generic wrapper matching Bank.API's ResponseDto&lt;T&gt; envelope.
    /// Used for deserialising all Bank.API responses.
    /// </summary>
    public class BankApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
