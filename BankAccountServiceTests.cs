using Bank.API.Data;
using Bank.API.Models;
using Bank.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bank.API.Tests
{
    /// <summary>
    /// Unit tests for <see cref="BankAccountService"/>.
    /// Uses EF Core In-Memory database.
    /// </summary>
    public class BankAccountServiceTests
    {
        private static BankDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new BankDbContext(options);
        }

        private static BankAccountService CreateService(BankDbContext context)
        {
            var logger = NullLogger<BankAccountService>.Instance;
            return new BankAccountService(context, logger);
        }

        [Fact]
        public async Task GetAccountByNumber_ExistingAccount_ReturnsBankAccountDto()
        {
            using var ctx = CreateContext(nameof(GetAccountByNumber_ExistingAccount_ReturnsBankAccountDto));
            ctx.BankAccounts.Add(new BankAccount
            {
                BankAccountId     = 1,
                AccountNumber     = "DEMO-BANK-001",
                AccountHolderName = "Sunil Perera",
                AvailableBalance  = 50000m,
                Currency          = "LKR",
                IsActive          = true,
                CreatedAt         = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var account = await svc.GetAccountByNumberAsync("DEMO-BANK-001");

            Assert.NotNull(account);
            Assert.Equal("DEMO-BANK-001", account.AccountNumber);
            Assert.Equal("Sunil Perera", account.AccountHolderName);
            Assert.Equal(50000m, account.AvailableBalance);
            Assert.Equal("LKR", account.Currency);
            Assert.True(account.IsActive);
        }

        [Fact]
        public async Task GetAccountByNumber_NonExistingAccount_ReturnsNull()
        {
            using var ctx = CreateContext(nameof(GetAccountByNumber_NonExistingAccount_ReturnsNull));
            var svc = CreateService(ctx);
            var account = await svc.GetAccountByNumberAsync("DEMO-BANK-999");

            Assert.Null(account);
        }

        [Fact]
        public async Task GetAllAccounts_ReturnsAllSeededAccounts()
        {
            using var ctx = CreateContext(nameof(GetAllAccounts_ReturnsAllSeededAccounts));
            ctx.BankAccounts.AddRange(
                new BankAccount { BankAccountId = 1, AccountNumber = "ACC-01", AccountHolderName = "User 1", AvailableBalance = 100m, Currency = "LKR" },
                new BankAccount { BankAccountId = 2, AccountNumber = "ACC-02", AccountHolderName = "User 2", AvailableBalance = 200m, Currency = "LKR" }
            );
            await ctx.SaveChangesAsync();

            var svc = CreateService(ctx);
            var accounts = (await svc.GetAllAccountsAsync()).ToList();

            Assert.Equal(2, accounts.Count);
            Assert.Contains(accounts, a => a.AccountNumber == "ACC-01");
            Assert.Contains(accounts, a => a.AccountNumber == "ACC-02");
        }
    }
}
