using Bank.API.Data;
using Bank.API.DTOs.Accounts;
using Bank.API.Interfaces;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Services
{
    /// <summary>
    /// Service implementation for managing bank account operations.
    /// </summary>
    public class BankAccountService : IBankAccountService
    {
        private readonly BankDbContext _context;
        private readonly ILogger<BankAccountService> _logger;

        public BankAccountService(BankDbContext context, ILogger<BankAccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<BankAccountDto?> GetAccountByNumberAsync(string accountNumber)
        {
            _logger.LogInformation("Retrieving bank account details for {AccountNumber}", accountNumber);

            var account = await _context.BankAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

            return account == null ? null : MapToDto(account);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BankAccountDto>> GetAllAccountsAsync()
        {
            _logger.LogInformation("Retrieving all bank accounts");

            var accounts = await _context.BankAccounts
                .AsNoTracking()
                .ToListAsync();

            return accounts.Select(MapToDto);
        }

        private static BankAccountDto MapToDto(BankAccount account) => new BankAccountDto
        {
            AccountNumber     = account.AccountNumber,
            AccountHolderName = account.AccountHolderName,
            AvailableBalance  = account.AvailableBalance,
            Currency          = account.Currency,
            IsActive          = account.IsActive,
            CreatedAt         = account.CreatedAt.ToString("o")
        };
    }
}
