using System;
using System.ComponentModel.DataAnnotations;

namespace NWSDB.API.Models
{
    public class WaterUsage
    {
        [Key]
        public int UsageId { get; set; }

        [Required]
        [MaxLength(20)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ReadingMonth { get; set; } = string.Empty;

        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Customer Customer { get; set; } = null!;
    }
}
