using Bank.API.Data;
using Bank.API.Interfaces;
using Bank.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Bank.API.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IBankTransactionRepository"/>.
    /// </summary>
    public class BankTransactionRepository : IBankTransactionRepository
    {
        private readonly BankDbContext _context;

        public BankTransactionRepository(BankDbContext context)
        {
            _context = context;
        }

        public async Task<BankTransaction?> GetByIdAsync(string transactionId)
        {
            return await _context.BankTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<BankTransaction?> GetByExternalReferenceAsync(string externalReference)
        {
            return await _context.BankTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ExternalReference == externalReference);
        }

        public async Task AddAsync(BankTransaction transaction)
        {
            await _context.BankTransactions.AddAsync(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
