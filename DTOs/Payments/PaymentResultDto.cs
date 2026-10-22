namespace Bank.API.DTOs.Payments
{
    /// <summary>
    /// Result returned after a payment is processed.
    /// </summary>
    public class PaymentResultDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "LKR";
        public string Message { get; set; } = string.Empty;
        public string ProcessedAt { get; set; } = string.Empty;
    }
}
