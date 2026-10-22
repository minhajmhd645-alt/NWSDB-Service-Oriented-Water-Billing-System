namespace NWSDB.API.DTOs.Admin
{
    public class AdminStatsDto
    {
        public int TotalCustomers { get; set; }
        public int TotalBills { get; set; }
        public int UnpaidBills { get; set; }
        public int PaidBills { get; set; }
        public int OverdueBills { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalRevenueCollected { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
    }
}
