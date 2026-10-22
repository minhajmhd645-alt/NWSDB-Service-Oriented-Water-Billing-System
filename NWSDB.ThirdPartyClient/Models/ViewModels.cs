using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWSDB.ThirdPartyClient.Models
{
    public class CustomerLookupViewModel
    {
        [Required(ErrorMessage = "Please enter an account number.")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        public CustomerDto? Customer { get; set; }
        public CustomerSummaryDto? Summary { get; set; }
        public IEnumerable<BillDto>? Bills { get; set; }
        public IEnumerable<BillDto>? UnpaidBills { get; set; }
        public WaterUsageDto? CurrentUsage { get; set; }
        public IEnumerable<PaymentDto>? PaymentHistory { get; set; }

        public string? ErrorMessage { get; set; }
        public bool IsNotFound { get; set; }
        public bool IsServiceUnavailable { get; set; }
    }

    public class PayBillViewModel
    {
        [Required]
        public int BillId { get; set; }

        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        public string BillingMonth { get; set; } = string.Empty;
        public System.DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "DemoBankAccount";

        [Required(ErrorMessage = "Please select a bank account to debit.")]
        [Display(Name = "Bank Account / Card")]
        public string BankAccountNumber { get; set; } = "DEMO-BANK-001";

        /// <summary>Display name of the payment channel selected before reaching this form.</summary>
        public string? ChannelName { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class PaymentResultViewModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        public PaymentDto? Payment { get; set; }
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal AttemptedAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
    }

    /// <summary>Carries bill context and the customer's channel selection through the payment-channel flow.</summary>
    public class PaymentChannelViewModel
    {
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BillingMonth { get; set; } = string.Empty;

        /// <summary>The channel key chosen by the customer, e.g. "NwsdbOfficialPortal", "Cargills", etc.</summary>
        public string? SelectedChannel { get; set; }

        /// <summary>Populated from config – used on the PartnerInfo page for the NWSDB portal button.</summary>
        public string NwsdbWebBaseUrl { get; set; } = string.Empty;
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string Message { get; set; } = "An unexpected error occurred.";
    }
}
