using System.ComponentModel.DataAnnotations;

namespace Bank.API.DTOs.Payments
{
    /// <summary>
    /// Request body for initiating a bank payment.
    /// </summary>
    public class PaymentRequestDto
    {
        /// <summary>
        /// Demo bank account number (e.g. DEMO-BANK-001).
        /// </summary>
        [Required(ErrorMessage = "Account number is required.")]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Reference identifying the bill being paid (e.g. NWSDB-BILL-10001).
        /// </summary>
        [Required(ErrorMessage = "Bill reference is required.")]
        [MaxLength(150)]
        public string BillReference { get; set; } = string.Empty;

        /// <summary>
        /// Payment amount. Must be greater than zero.
        /// </summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Three-letter currency code (e.g. LKR). Default is LKR.
        /// </summary>
        [MaxLength(10)]
        public string Currency { get; set; } = "LKR";

        /// <summary>
        /// Optional description for the payment transaction.
        /// </summary>
        [MaxLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Payment method. Allowed values: DemoBankAccount, DemoCard.
        /// </summary>
        [Required(ErrorMessage = "Payment method is required.")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// External correlation reference supplied by the calling system (e.g. NWSDB-PAY-001).
        /// Must be unique per payment.
        /// </summary>
        [Required(ErrorMessage = "External reference is required.")]
        [MaxLength(150)]
        public string ExternalReference { get; set; } = string.Empty;
    }
}
