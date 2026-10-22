using Bank.API.Models;

namespace Bank.API.Data
{
    /// <summary>
    /// Seeds the BankDb with fictional demonstration data.
    /// All accounts and transactions are purely fictional.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(BankDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed BankAccounts only if table is empty
            if (!context.BankAccounts.Any())
            {
                var accounts = new List<BankAccount>
                {
                    new BankAccount
                    {
                        AccountNumber  = "DEMO-BANK-001",
                        AccountHolderName = "Sunil Perera",
                        AvailableBalance  = 50000.00m,
                        Currency   = "LKR",
                        IsActive  = true,
                        CreatedAt = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new BankAccount
                    {
                        AccountNumber  = "DEMO-BANK-002",
                        AccountHolderName = "Nimal Silva",
                        AvailableBalance  = 12000.00m,
                        Currency   = "LKR",
                        IsActive  = true,
                        CreatedAt = new DateTime(2024, 2, 15, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new BankAccount
                    {
                        AccountNumber  = "DEMO-BANK-003",
                        AccountHolderName = "Kamala Fernando",
                        AvailableBalance  = 75000.00m,
                        Currency   = "LKR",
                        IsActive  = true,
                        CreatedAt = new DateTime(2024, 3, 5, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new BankAccount
                    {
                        AccountNumber  = "DEMO-BANK-004",
                        AccountHolderName = "Roshan Jayawardena",
                        AvailableBalance  = 500.00m,   // low balance – useful for testing
                        Currency   = "LKR",
                        IsActive  = true,
                        CreatedAt = new DateTime(2024, 4, 20, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new BankAccount
                    {
                        AccountNumber  = "DEMO-BANK-005",
                        AccountHolderName = "Priya Rathnayake",
                        AvailableBalance  = 30000.00m,
                        Currency   = "LKR",
                        IsActive  = false,  // inactive – useful for testing
                        CreatedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
                };

                context.BankAccounts.AddRange(accounts);
                context.SaveChanges();
            }

            // Seed BankTransactions only if table is empty
            if (!context.BankTransactions.Any())
            {
                var transactions = new List<BankTransaction>
                {
                    new BankTransaction
                    {
                        TransactionId     = "BANK-TXN-00001",
                        ExternalReference = "NWSDB-PAY-SEED-001",
                        AccountNumber     = "DEMO-BANK-001",
                        BillReference     = "NWSDB-BILL-10001",
                        Amount            = 2500.00m,
                        Currency          = "LKR",
                        Description       = "Water bill payment",
                        PaymentMethod     = "DemoBankAccount",
                        Status            = "Success",
                        TransactionDate   = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc),
                        CreatedAt         = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc)
                    },
                    new BankTransaction
                    {
                        TransactionId     = "BANK-TXN-00002",
                        ExternalReference = "NWSDB-PAY-SEED-002",
                        AccountNumber     = "DEMO-BANK-002",
                        BillReference     = "NWSDB-BILL-10002",
                        Amount            = 1800.00m,
                        Currency          = "LKR",
                        Description       = "Water bill payment",
                        PaymentMethod     = "DemoCard",
                        Status            = "Success",
                        TransactionDate   = new DateTime(2026, 8, 5, 10, 30, 0, DateTimeKind.Utc),
                        CreatedAt         = new DateTime(2026, 8, 5, 10, 30, 0, DateTimeKind.Utc)
                    },
                    new BankTransaction
                    {
                        TransactionId     = "BANK-TXN-00003",
                        ExternalReference = "NWSDB-PAY-SEED-003",
                        AccountNumber     = "DEMO-BANK-004",
                        BillReference     = "NWSDB-BILL-10003",
                        Amount            = 3200.00m,
                        Currency          = "LKR",
                        Description       = "Water bill payment",
                        PaymentMethod     = "DemoBankAccount",
                        Status            = "Failed",    // insufficient balance
                        TransactionDate   = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc),
                        CreatedAt         = new DateTime(2026, 8, 10, 14, 0, 0, DateTimeKind.Utc)
                    },
                    new BankTransaction
                    {
                        TransactionId     = "BANK-TXN-00004",
                        ExternalReference = "NWSDB-PAY-SEED-004",
                        AccountNumber     = "DEMO-BANK-003",
                        BillReference     = "NWSDB-BILL-10004",
                        Amount            = 4750.00m,
                        Currency          = "LKR",
                        Description       = "Water bill payment",
                        PaymentMethod     = "DemoCard",
                        Status            = "Success",
                        TransactionDate   = new DateTime(2026, 9, 1, 8, 15, 0, DateTimeKind.Utc),
                        CreatedAt         = new DateTime(2026, 9, 1, 8, 15, 0, DateTimeKind.Utc)
                    }
                };

                context.BankTransactions.AddRange(transactions);
                context.SaveChanges();
            }
        }
    }
}
