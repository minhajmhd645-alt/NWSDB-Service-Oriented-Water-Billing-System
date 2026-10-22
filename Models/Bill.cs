using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NWSDB.API.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string BillingMonth { get; set; } = string.Empty;

        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BillAmount { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, Overdue

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Customer Customer { get; set; } = null!;
    }
}
