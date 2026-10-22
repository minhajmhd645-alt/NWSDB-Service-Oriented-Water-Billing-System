using System;

namespace NWSDB.API.DTOs.Bills
{
    public class BillDto
    {
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }
        public decimal BillAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
