using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWSDB.Web.Models
{
    public class AccountLookupViewModel
    {
        [Required(ErrorMessage = "Please enter your NWSDB account number.")]
        [Display(Name = "NWSDB Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }

    public class AccountSummaryViewModel
    {
        public CustomerModel Customer { get; set; } = new();
        public decimal CurrentOutstandingAmount { get; set; }
        public int NumberOfUnpaidBills { get; set; }
        public WaterUsageModel? CurrentWaterUsage { get; set; }
        public PaymentModel? LatestPayment { get; set; }
        public BillModel? LatestBill { get; set; }
    }

    public class BillsListViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public List<BillModel> Bills { get; set; } = new();
        public bool IsUnpaidOnly { get; set; }
    }

    public class PaymentFormViewModel
    {
        public int BillId { get; set; }

        public string AccountNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public decimal BillAmount { get; set; }

        public string BillingMonth { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public string PaymentMethod { get; set; } = "DemoBankAccount";

        [Required(ErrorMessage = "Please enter your demo bank account number.")]
        [Display(Name = "Bank Account Number")]
        public string BankAccountNumber { get; set; } = "DEMO-BANK-001";

        public string? ErrorMessage { get; set; }
    }

    public class PaymentReceiptViewModel
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

    public class PaymentFailureViewModel
    {
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
