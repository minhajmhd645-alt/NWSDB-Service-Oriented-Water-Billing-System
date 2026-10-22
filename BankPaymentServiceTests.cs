using Bank.API.Data;
using Bank.API.DTOs.Payments;
using Bank.API.Models;
using Bank.API.Repositories;
using Bank.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bank.API.Tests
{
    /// <summary>
    /// Unit tests for <see cref="BankPaymentService"/>.
    /// Uses EF Core In-Memory database – no SQL Server required.
    /// </summary>
    public class BankPaymentServiceTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────

        private static BankDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new BankDbContext(options);
        }

        private static BankPaymentService CreateService(BankDbContext context)
        {
            var repo   = new BankTransactionRepository(context);
            var logger = NullLogger<BankPaymentService>.Instance;
            return new BankPaymentService(context, repo, logger);
        }

        private static BankAccount ActiveAccount(string accountNumber = "DEMO-BANK-001",
            decimal balance = 50000m) => new BankAccount
        {
            BankAccountId     = 1,
            AccountNumber     = accountNumber,
            AccountHolderName = "Test Holder",
            AvailableBalance  = balance,
            Currency          = "LKR",
            IsActive          = true,
            CreatedAt         = DateTime.UtcNow
        };

        private static BankAccount InactiveAccount() => new BankAccount
        {
            BankAccountId     = 2,
            AccountNumber     = "DEMO-BANK-INACTIVE",
            AccountHolderName = "Inactive Holder",
            AvailableBalance  = 10000m,
            Currency          = "LKR",
            IsActive          = false,
            CreatedAt         = DateTime.UtcNow
        };

        private static PaymentRequestDto ValidRequest(
            string accountNumber  = "DEMO-BANK-001",
            decimal amount        = 1500m,
            string paymentMethod  = "DemoBankAccount",
            string externalRef    = "NWSDB-PAY-TEST-001") => new PaymentRequestDto
        {
            AccountNumber     = accountNumber,
            BillReference     = "NWSDB-BILL-TEST-001",
            Amount            = amount,
            PaymentMethod     = paymentMethod,
            ExternalReference = externalRef
        };

        // ── Test: Valid payment succeeds ───────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_ValidBankAccount_ReturnsSuccess()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_ValidBankAccount_ReturnsSuccess));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc    = CreateService(ctx);
            var result = await svc.ProcessPaymentAsync(ValidRequest());

            Assert.NotNull(result);
            Assert.Equal("Success", result.Status);
            Assert.Equal(1500m, result.Amount);
            Assert.StartsWith("BANK-TXN-", result.TransactionId);
            Assert.NotEmpty(result.ProcessedAt);
        }

        // ── Test: Balance deducted after payment ──────────────────────────────

        [Fact]
        public async Task ProcessPayment_ValidPayment_DeductsBalance()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_ValidPayment_DeductsBalance));
            var account = ActiveAccount(balance: 5000m);
            ctx.BankAccounts.Add(account);
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.ProcessPaymentAsync(ValidRequest(amount: 2000m));

            var updatedAccount = await ctx.BankAccounts
                .FirstAsync(a => a.AccountNumber == "DEMO-BANK-001");

            Assert.Equal(3000m, updatedAccount.AvailableBalance);
        }

        // ── Test: DemoCard payment method also works ──────────────────────────

        [Fact]
        public async Task ProcessPayment_DemoCardMethod_ReturnsSuccess()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_DemoCardMethod_ReturnsSuccess));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc    = CreateService(ctx);
            var result = await svc.ProcessPaymentAsync(ValidRequest(paymentMethod: "DemoCard"));

            Assert.Equal("Success", result.Status);
        }

        // ── Test: Invalid account number ──────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_InvalidAccountNumber_ThrowsArgumentException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_InvalidAccountNumber_ThrowsArgumentException));
            // No accounts seeded

            var svc = CreateService(ctx);
            var ex  = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.ProcessPaymentAsync(ValidRequest(accountNumber: "DEMO-BANK-NOTEXIST")));

            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Inactive account ────────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_InactiveAccount_ThrowsArgumentException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_InactiveAccount_ThrowsArgumentException));
            ctx.BankAccounts.Add(InactiveAccount());
            await ctx.SaveChangesAsync();

            var svc     = CreateService(ctx);
            var request = ValidRequest(accountNumber: "DEMO-BANK-INACTIVE");
            var ex      = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.ProcessPaymentAsync(request));

            Assert.Contains("not active", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Zero amount ────────────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_ZeroAmount_ThrowsArgumentException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_ZeroAmount_ThrowsArgumentException));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var ex  = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.ProcessPaymentAsync(ValidRequest(amount: 0m)));

            Assert.Contains("greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Negative amount ────────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_NegativeAmount_ThrowsArgumentException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_NegativeAmount_ThrowsArgumentException));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var ex  = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.ProcessPaymentAsync(ValidRequest(amount: -500m)));

            Assert.Contains("greater than zero", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Insufficient balance ────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_InsufficientBalance_ThrowsArgumentException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_InsufficientBalance_ThrowsArgumentException));
            ctx.BankAccounts.Add(ActiveAccount(balance: 100m));
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var ex  = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.ProcessPaymentAsync(ValidRequest(amount: 5000m)));

            Assert.Contains("Insufficient balance", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Invalid payment method ──────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_InvalidPaymentMethod_ThrowsArgumentException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_InvalidPaymentMethod_ThrowsArgumentException));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var ex  = await Assert.ThrowsAsync<ArgumentException>(() =>
                svc.ProcessPaymentAsync(ValidRequest(paymentMethod: "Visa")));

            Assert.Contains("Invalid payment method", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Duplicate external reference ────────────────────────────────

        [Fact]
        public async Task ProcessPayment_DuplicateExternalReference_ThrowsInvalidOperationException()
        {
            using var ctx = CreateContext(nameof(ProcessPayment_DuplicateExternalReference_ThrowsInvalidOperationException));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);

            // First payment succeeds
            await svc.ProcessPaymentAsync(ValidRequest(externalRef: "NWSDB-PAY-DUPE-001"));

            // Second with the same external reference should fail
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.ProcessPaymentAsync(ValidRequest(externalRef: "NWSDB-PAY-DUPE-001")));

            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Test: Get transaction by ID ───────────────────────────────────────

        [Fact]
        public async Task GetTransactionById_ExistingTransaction_ReturnsTransactionDto()
        {
            using var ctx = CreateContext(nameof(GetTransactionById_ExistingTransaction_ReturnsTransactionDto));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc    = CreateService(ctx);
            var result = await svc.ProcessPaymentAsync(ValidRequest());

            var dto = await svc.GetTransactionByIdAsync(result.TransactionId);

            Assert.NotNull(dto);
            Assert.Equal(result.TransactionId, dto.TransactionId);
            Assert.Equal("Success", dto.Status);
            Assert.Equal(1500m, dto.Amount);
        }

        // ── Test: Get transaction by ID – not found ───────────────────────────

        [Fact]
        public async Task GetTransactionById_NonExistingId_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(GetTransactionById_NonExistingId_ReturnsNull));
            var svc = CreateService(ctx);

            var dto = await svc.GetTransactionByIdAsync("BANK-TXN-DOES-NOT-EXIST");

            Assert.Null(dto);
        }

        // ── Test: Get transaction by external reference ───────────────────────

        [Fact]
        public async Task GetTransactionByExternalReference_Existing_ReturnsTransactionDto()
        {
            using var ctx = CreateContext(nameof(GetTransactionByExternalReference_Existing_ReturnsTransactionDto));
            ctx.BankAccounts.Add(ActiveAccount());
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            await svc.ProcessPaymentAsync(ValidRequest(externalRef: "NWSDB-PAY-REF-LOOKUP"));

            var dto = await svc.GetTransactionByExternalReferenceAsync("NWSDB-PAY-REF-LOOKUP");

            Assert.NotNull(dto);
            Assert.Equal("NWSDB-PAY-REF-LOOKUP", dto.ExternalReference);
            Assert.Equal("Success", dto.Status);
        }

        // ── Test: Get transaction by external reference – not found ───────────

        [Fact]
        public async Task GetTransactionByExternalReference_NonExisting_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(GetTransactionByExternalReference_NonExisting_ReturnsNull));
            var svc = CreateService(ctx);

            var dto = await svc.GetTransactionByExternalReferenceAsync("NWSDB-PAY-GHOST");

            Assert.Null(dto);
        }
    }
}
