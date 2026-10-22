namespace Bank.API.DTOs.Payments
{
    /// <summary>
    /// Full transaction details returned by lookup endpoints.
    /// </summary>
    public class TransactionDto
    {
        public string TransactionId { get; set; } = string.Empty;
        public string ExternalReference { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BillReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "LKR";
        public string? Description { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
