using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank.API.Models
{
    /// <summary>
    /// Represents a fictional demo bank account used for payment simulation.
    /// Does NOT store real banking credentials or sensitive financial data.
    /// </summary>
    public class BankAccount
    {
        [Key]
        public int BankAccountId { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string AccountHolderName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AvailableBalance { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "LKR";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    }
}
