using Bank.API.Models;

namespace Bank.API.Interfaces
{
    /// <summary>
    /// Repository contract for bank transaction data access.
    /// </summary>
    public interface IBankTransactionRepository
    {
        Task<BankTransaction?> GetByIdAsync(string transactionId);
        Task<BankTransaction?> GetByExternalReferenceAsync(string externalReference);
        Task AddAsync(BankTransaction transaction);
        Task SaveChangesAsync();
    }
}
