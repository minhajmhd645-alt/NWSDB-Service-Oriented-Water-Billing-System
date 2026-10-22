using System;

namespace NWSDB.Web.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CustomerModel
    {
        public int CustomerId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class WaterUsageModel
    {
        public string ReadingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class BillModel
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

    public class PaymentModel
    {
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    public class CustomerSummaryModel
    {
        public CustomerModel Customer { get; set; } = new();
        public decimal CurrentOutstandingAmount { get; set; }
        public int NumberOfUnpaidBills { get; set; }
        public WaterUsageModel? CurrentWaterUsage { get; set; }
        public PaymentModel? LatestPayment { get; set; }
        public BillModel? LatestBill { get; set; }
    }

    public class PaymentRequestModel
    {
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "DemoBankAccount";
        public string BankAccountNumber { get; set; } = string.Empty;
    }

    public class CreateCustomerRequestModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateCustomerRequestModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateBillRequestModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string BillingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public decimal BillAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Unpaid";
    }

    public class UpdateBillRequestModel
    {
        public string BillingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public decimal BillAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Unpaid";
    }

    public class AdminStatsModel
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
