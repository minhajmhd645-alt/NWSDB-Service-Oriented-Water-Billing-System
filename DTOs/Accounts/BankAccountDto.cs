namespace Bank.API.DTOs.Accounts
{
    /// <summary>
    /// Data transfer object representing a demo bank account.
    /// Exposes public account information without revealing sensitive internal details.
    /// </summary>
    public class BankAccountDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "LKR";
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }
}
