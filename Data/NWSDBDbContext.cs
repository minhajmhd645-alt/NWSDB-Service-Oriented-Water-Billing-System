using Microsoft.EntityFrameworkCore;
using NWSDB.API.Models;

namespace NWSDB.API.Data
{
    public class NWSDBDbContext : DbContext
    {
        public NWSDBDbContext(DbContextOptions<NWSDBDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;
        public DbSet<WaterUsage> WaterUsages { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.AccountNumber)
                .IsUnique();

            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bills)
                .HasForeignKey(b => b.AccountNumber)
                .HasPrincipalKey(c => c.AccountNumber);

            modelBuilder.Entity<WaterUsage>()
                .HasOne(u => u.Customer)
                .WithMany(c => c.WaterUsages)
                .HasForeignKey(u => u.AccountNumber)
                .HasPrincipalKey(c => c.AccountNumber);
                
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Bill)
                .WithMany()
                .HasForeignKey(p => p.BillId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
