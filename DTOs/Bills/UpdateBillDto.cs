using System;
using System.ComponentModel.DataAnnotations;

namespace NWSDB.API.DTOs.Bills
{
    public class UpdateBillDto
    {
        [Required(ErrorMessage = "Billing month is required.")]
        [MaxLength(20)]
        public string BillingMonth { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Previous reading must be zero or greater.")]
        public int PreviousReading { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current reading must be zero or greater.")]
        public int CurrentReading { get; set; }

        [Range(0.01, 1000000.00, ErrorMessage = "Bill amount must be greater than zero.")]
        public decimal BillAmount { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(20)]
        public string Status { get; set; } = "Unpaid";
    }
}
