using Microsoft.EntityFrameworkCore;
using Bank.API.Models;

namespace Bank.API.Data
{
    public class BankDbContext : DbContext
    {
        public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
        {
        }

        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<BankTransaction> BankTransactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BankAccount – unique account number index
            modelBuilder.Entity<BankAccount>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            // BankTransaction – unique external reference index
            modelBuilder.Entity<BankTransaction>()
                .HasIndex(t => t.ExternalReference)
                .IsUnique();

            // BankTransaction → BankAccount (FK via AccountNumber)
            modelBuilder.Entity<BankTransaction>()
                .HasOne(t => t.BankAccount)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountNumber)
                .HasPrincipalKey(a => a.AccountNumber)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
