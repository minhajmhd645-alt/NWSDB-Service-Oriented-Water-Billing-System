using Bank.API.Data;
using Bank.API.DTOs.Payments;
using Bank.API.Interfaces;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Services
{
    /// <summary>
    /// Implements <see cref="IBankPaymentService"/> to simulate bank payment processing.
    /// This is a demonstration service; no real financial transactions are performed.
    /// </summary>
    public class BankPaymentService : IBankPaymentService
    {
        private static readonly HashSet<string> ValidPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "DemoBankAccount",
            "DemoCard"
        };

        private readonly BankDbContext _context;
        private readonly IBankTransactionRepository _transactionRepository;
        private readonly ILogger<BankPaymentService> _logger;

        public BankPaymentService(
            BankDbContext context,
            IBankTransactionRepository transactionRepository,
            ILogger<BankPaymentService> logger)
        {
            _context = context;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentRequestDto request)
        {
            _logger.LogInformation(
                "Processing payment for account {AccountNumber}, external ref {ExternalReference}, amount {Amount}",
                request.AccountNumber, request.ExternalReference, request.Amount);

            // 1. Validate payment method
            if (!ValidPaymentMethods.Contains(request.PaymentMethod))
            {
                throw new ArgumentException(
                    $"Invalid payment method '{request.PaymentMethod}'. Allowed values: DemoBankAccount, DemoCard.");
            }

            // 2. Validate amount
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.");
            }

            // 3. Validate external reference not empty
            if (string.IsNullOrWhiteSpace(request.ExternalReference))
            {
                throw new ArgumentException("External reference is required.");
            }

            // 4. Check for duplicate external reference (idempotency guard)
            var existingTransaction = await _transactionRepository
                .GetByExternalReferenceAsync(request.ExternalReference);

            if (existingTransaction != null)
            {
                throw new InvalidOperationException(
                    $"A transaction with external reference '{request.ExternalReference}' already exists " +
                    $"(TransactionId: {existingTransaction.TransactionId}).");
            }

            // 5. Validate bank account exists
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);

            if (account == null)
            {
                throw new ArgumentException(
                    $"Bank account '{request.AccountNumber}' was not found.");
            }

            // 6. Validate account is active
            if (!account.IsActive)
            {
                throw new ArgumentException(
                    $"Bank account '{request.AccountNumber}' is not active.");
            }

            // 7. Validate sufficient balance
            if (account.AvailableBalance < request.Amount)
            {
                throw new ArgumentException(
                    $"Insufficient balance. Available: {account.AvailableBalance:F2}, Requested: {request.Amount:F2}.");
            }

            // 8. Process payment – deduct balance and create transaction
            var transactionId = GenerateTransactionId();
            var now = DateTime.UtcNow;
            var currency = string.IsNullOrWhiteSpace(request.Currency) ? "LKR" : request.Currency.ToUpperInvariant();

            account.AvailableBalance -= request.Amount;

            var transaction = new BankTransaction
            {
                TransactionId     = transactionId,
                ExternalReference = request.ExternalReference,
                AccountNumber     = request.AccountNumber,
                BillReference     = request.BillReference,
                Amount            = request.Amount,
                Currency          = currency,
                Description       = request.Description,
                PaymentMethod     = request.PaymentMethod,
                Status            = "Success",
                TransactionDate   = now,
                CreatedAt         = now
            };

            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Payment processed successfully. TransactionId: {TransactionId}", transactionId);

            return new PaymentResultDto
            {
                TransactionId = transactionId,
                Status        = "Success",
                Amount        = request.Amount,
                Currency      = currency,
                Message       = "Payment processed successfully.",
                ProcessedAt   = now.ToString("o")
            };
        }

        /// <inheritdoc/>
        public async Task<TransactionDto?> GetTransactionByIdAsync(string transactionId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            return transaction == null ? null : MapToDto(transaction);
        }

        /// <inheritdoc/>
        public async Task<TransactionDto?> GetTransactionByExternalReferenceAsync(string externalReference)
        {
            var transaction = await _transactionRepository
                .GetByExternalReferenceAsync(externalReference);
            return transaction == null ? null : MapToDto(transaction);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static string GenerateTransactionId()
        {
            // Format: BANK-TXN-<timestamp><random> for uniqueness and readability
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"BANK-TXN-{timestamp}-{random}";
        }

        private static TransactionDto MapToDto(BankTransaction t) => new TransactionDto
        {
            TransactionId     = t.TransactionId,
            ExternalReference = t.ExternalReference,
            AccountNumber     = t.AccountNumber,
            BillReference     = t.BillReference,
            Amount            = t.Amount,
            Currency          = t.Currency,
            Description       = t.Description,
            PaymentMethod     = t.PaymentMethod,
            Status            = t.Status,
            TransactionDate   = t.TransactionDate.ToString("o"),
            CreatedAt         = t.CreatedAt.ToString("o")
        };
    }
}
