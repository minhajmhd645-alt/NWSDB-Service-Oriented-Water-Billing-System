using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NWSDB.API.Data;
using NWSDB.API.DTOs.Payments;
using NWSDB.API.Interfaces;
using NWSDB.API.Models;

namespace NWSDB.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly NWSDBDbContext _context;
        private readonly IBankApiClient _bankApiClient;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            NWSDBDbContext context,
            IBankApiClient bankApiClient,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _bankApiClient = bankApiClient;
            _logger = logger;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _context.Payments
                .OrderByDescending(p => p.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                BillId = p.BillId,
                AccountNumber = p.AccountNumber,
                Amount = p.Amount,
                TransactionId = p.TransactionId,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            });
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsAsync(string accountNumber)
        {
            var payments = await _context.Payments
                .Where(p => p.AccountNumber == accountNumber)
                .OrderByDescending(p => p.PaymentDate)
                .AsNoTracking()
                .ToListAsync();

            return payments.Select(p => new PaymentDto
            {
                PaymentId = p.PaymentId,
                BillId = p.BillId,
                AccountNumber = p.AccountNumber,
                Amount = p.Amount,
                TransactionId = p.TransactionId,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            });
        }

        public async Task<PaymentDto?> GetPaymentAsync(int paymentId)
        {
            var p = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PaymentId == paymentId);

            if (p == null) return null;

            return new PaymentDto
            {
                PaymentId = p.PaymentId,
                BillId = p.BillId,
                AccountNumber = p.AccountNumber,
                Amount = p.Amount,
                TransactionId = p.TransactionId,
                PaymentMethod = p.PaymentMethod,
                Status = p.Status,
                PaymentDate = p.PaymentDate
            };
        }

        public async Task<PaymentDto> CreatePaymentAsync(PaymentRequestDto request)
        {
            // ── NWSDB Validations ─────────────────────────────────────────────
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.AccountNumber == request.AccountNumber);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.BillId == request.BillId);
            if (bill == null)
                throw new ArgumentException("Bill not found.");

            if (bill.AccountNumber != request.AccountNumber)
                throw new ArgumentException("Bill does not belong to the specified account.");

            if (request.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");

            if (request.Amount != bill.BillAmount)
                throw new ArgumentException("Payment amount must match the bill amount.");

            if (bill.Status == "Paid")
                throw new ArgumentException("This bill is already paid.");

            var validMethods = new[] { "DemoCard", "DemoBankAccount" };
            if (!validMethods.Contains(request.PaymentMethod))
                throw new ArgumentException("Invalid payment method. Allowed: DemoCard, DemoBankAccount.");

            if (string.IsNullOrWhiteSpace(request.BankAccountNumber))
                throw new ArgumentException("Bank account number is required to process the payment.");

            // ── Generate a unique external reference for cross-system correlation ──
            var externalReference = "NWSDB-PAY-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            var billReference = $"NWSDB-BILL-{bill.BillId}";

            _logger.LogInformation(
                "Forwarding payment to Bank.API | BillId: {BillId} | BankAccount: {BankAccountNumber} | Amount: {Amount} | ExternalRef: {ExternalReference}",
                request.BillId, request.BankAccountNumber, request.Amount, externalReference);

            // ── Call Bank.API ─────────────────────────────────────────────────
            // Throws InvalidOperationException if the bank rejects the payment.
            // Throws InvalidOperationException on network failure.
            // No NWSDB DB writes happen before this succeeds.
            var bankRequest = new BankPaymentRequest
            {
                AccountNumber = request.BankAccountNumber,
                BillReference = billReference,
                Amount = request.Amount,
                Currency = "LKR",
                Description = $"NWSDB water bill payment for account {request.AccountNumber}, billing month {bill.BillingMonth}",
                PaymentMethod = request.PaymentMethod,
                ExternalReference = externalReference
            };

            var bankResult = await _bankApiClient.ProcessPaymentAsync(bankRequest);

            // ── Persist NWSDB Payment (only after bank confirms success) ──────
            var payment = new Payment
            {
                BillId = request.BillId,
                AccountNumber = request.AccountNumber,
                Amount = request.Amount,
                TransactionId = bankResult.TransactionId,   // bank's transaction ID stored here
                PaymentMethod = request.PaymentMethod,
                Status = "Success",
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update Bill status
            bill.Status = "Paid";

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Payment recorded in NWSDB DB | PaymentId: {PaymentId} | BankTransactionId: {TransactionId}",
                payment.PaymentId, payment.TransactionId);

            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                BillId = payment.BillId,
                AccountNumber = payment.AccountNumber,
                Amount = payment.Amount,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate
            };
        }
    }
}
