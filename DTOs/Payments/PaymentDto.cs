using System;

namespace NWSDB.API.DTOs.Payments
{
    public class PaymentDto
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

    public class PaymentRequestDto
    {
        public int BillId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Bank account number to debit (e.g. DEMO-BANK-001).
        /// Required for DemoBankAccount and DemoCard payment methods.
        /// </summary>
        public string BankAccountNumber { get; set; } = string.Empty;
    }
}
