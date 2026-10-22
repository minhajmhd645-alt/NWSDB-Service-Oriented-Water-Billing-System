using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWSDB.API.Data;
using NWSDB.API.Models;
using NWSDB.API.Services;
using Xunit;

namespace NWSDB.API.Tests
{
    public class BillServiceTests
    {
        private NWSDBDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<NWSDBDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new NWSDBDbContext(options);
        }

        private static Customer SampleCustomer() => new Customer
        {
            CustomerId = 1,
            AccountNumber = "NWSDB10001",
            FullName = "Sunil Perera",
            Address = "Colombo 03",
            PhoneNumber = "0711111111",
            Email = "sunil@example.com"
        };

        private static Bill UnpaidBill() => new Bill
        {
            BillId = 1,
            AccountNumber = "NWSDB10001",
            BillingMonth = "Sep-2026",
            PreviousReading = 100,
            CurrentReading = 150,
            UnitsConsumed = 50,
            BillAmount = 1500,
            DueDate = DateTime.UtcNow.AddDays(10),
            Status = "Unpaid"
        };

        private static Bill PaidBill() => new Bill
        {
            BillId = 2,
            AccountNumber = "NWSDB10001",
            BillingMonth = "Aug-2026",
            PreviousReading = 50,
            CurrentReading = 100,
            UnitsConsumed = 50,
            BillAmount = 1500,
            DueDate = DateTime.UtcNow.AddDays(-10),
            Status = "Paid"
        };

        [Fact]
        public async Task GetBillsAsync_ExistingAccount_ReturnsBills()
        {
            using var ctx = CreateInMemoryContext(nameof(GetBillsAsync_ExistingAccount_ReturnsBills));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            ctx.Bills.Add(PaidBill());
            await ctx.SaveChangesAsync();

            var svc = new BillService(ctx);
            var result = await svc.GetBillsAsync("NWSDB10001");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetBillsAsync_NoAccount_ReturnsEmpty()
        {
            using var ctx = CreateInMemoryContext(nameof(GetBillsAsync_NoAccount_ReturnsEmpty));

            var svc = new BillService(ctx);
            var result = await svc.GetBillsAsync("NWSDB99999");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUnpaidBillsAsync_ReturnsOnlyUnpaid()
        {
            using var ctx = CreateInMemoryContext(nameof(GetUnpaidBillsAsync_ReturnsOnlyUnpaid));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            ctx.Bills.Add(PaidBill());
            await ctx.SaveChangesAsync();

            var svc = new BillService(ctx);
            var result = await svc.GetUnpaidBillsAsync("NWSDB10001");

            Assert.Single(result);
            Assert.Equal("Unpaid", result.First().Status);
        }

        [Fact]
        public async Task GetBillAsync_ExistingId_ReturnsBill()
        {
            using var ctx = CreateInMemoryContext(nameof(GetBillAsync_ExistingId_ReturnsBill));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            await ctx.SaveChangesAsync();

            var svc = new BillService(ctx);
            var result = await svc.GetBillAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.BillId);
        }

        [Fact]
        public async Task GetBillAsync_NonExistingId_ReturnsNull()
        {
            using var ctx = CreateInMemoryContext(nameof(GetBillAsync_NonExistingId_ReturnsNull));

            var svc = new BillService(ctx);
            var result = await svc.GetBillAsync(9999);

            Assert.Null(result);
        }
    }
}
