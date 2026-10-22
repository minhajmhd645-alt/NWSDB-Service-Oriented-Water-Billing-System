using Bank.API.DTOs.Accounts;

namespace Bank.API.Interfaces
{
    /// <summary>
    /// Service contract for managing and retrieving demo bank account information.
    /// </summary>
    public interface IBankAccountService
    {
        /// <summary>
        /// Retrieves demo bank account details by account number.
        /// </summary>
        /// <param name="accountNumber">The account number (e.g. DEMO-BANK-001).</param>
        /// <returns>A <see cref="BankAccountDto"/> or null if not found.</returns>
        Task<BankAccountDto?> GetAccountByNumberAsync(string accountNumber);

        /// <summary>
        /// Retrieves all available demo bank accounts.
        /// </summary>
        /// <returns>A collection of <see cref="BankAccountDto"/>.</returns>
        Task<IEnumerable<BankAccountDto>> GetAllAccountsAsync();
    }
}
