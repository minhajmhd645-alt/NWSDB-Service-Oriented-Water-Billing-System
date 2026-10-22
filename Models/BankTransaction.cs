using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.API.Models
{
    /// <summary>
    /// Represents a bank payment transaction record.
    /// Statuses: Pending, Success, Failed
    /// Payment methods: DemoCard, DemoBankAccount
    /// </summary>
    public class BankTransaction
    {
        [Key]
        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// The reference ID supplied by the calling system (e.g., NWSDB API).
        /// Used to correlate transactions across systems.
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string ExternalReference { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BillReference { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "LKR";

        [MaxLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Demo payment method. Allowed values: DemoCard, DemoBankAccount.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Transaction status. Allowed values: Pending, Success, Failed.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public BankAccount? BankAccount { get; set; }
    }
}
