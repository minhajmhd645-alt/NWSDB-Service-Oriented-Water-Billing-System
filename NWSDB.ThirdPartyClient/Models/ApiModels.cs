using System;
using System.Collections.Generic;

namespace NWSDB.ThirdPartyClient.Models
{
    public class ResponseDto<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string Timestamp { get; set; } = string.Empty;
    }

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
        public WaterUsageDto? CurrentWaterUsage { get; set; }
        public PaymentDto? LatestPayment { get; set; }
        public BillDto? LatestBill { get; set; }
    }

    public class BillDto
    {
        public int BillId { get; set; }
        public string BillingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }
        public decimal BillAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class WaterUsageDto
    {
        public string ReadingMonth { get; set; } = string.Empty;
        public int PreviousReading { get; set; }
        public int CurrentReading { get; set; }
        public int UnitsConsumed { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int BillId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
    }

    public class PaymentRequestDto
    {
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
    }
}
