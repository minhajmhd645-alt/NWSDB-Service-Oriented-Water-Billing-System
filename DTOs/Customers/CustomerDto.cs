using System.Collections.Generic;

namespace NWSDB.API.DTOs.Customers
{
    public class CustomerDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CustomerSummaryDto
    {
        public CustomerDto Customer { get; set; } = null!;
        public decimal CurrentOutstandingAmount { get; set; }
        public int NumberOfUnpaidBills { get; set; }
        public WaterUsage.WaterUsageDto? CurrentWaterUsage { get; set; }
        public Payments.PaymentDto? LatestPayment { get; set; }
        public Bills.BillDto? LatestBill { get; set; }
    }
}
