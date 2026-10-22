using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWSDB.API.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int BillId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // DemoCard, DemoBankAccount

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Success, Failed

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Bill Bill { get; set; } = null!;
    }
}
