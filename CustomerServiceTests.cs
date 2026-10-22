using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWSDB.API.Data;
using NWSDB.API.Models;
using NWSDB.API.Services;
using Xunit;

namespace NWSDB.API.Tests
{
    public class CustomerServiceTests
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

        [Fact]
        public async Task GetCustomerAsync_ExistingAccount_ReturnsCustomer()
        {
            using var ctx = CreateInMemoryContext(nameof(GetCustomerAsync_ExistingAccount_ReturnsCustomer));
            ctx.Customers.Add(SampleCustomer());
            await ctx.SaveChangesAsync();

            var svc = new CustomerService(ctx);
            var result = await svc.GetCustomerAsync("NWSDB10001");

            Assert.NotNull(result);
            Assert.Equal("NWSDB10001", result.AccountNumber);
            Assert.Equal("Sunil Perera", result.FullName);
        }

        [Fact]
        public async Task GetCustomerAsync_NonExistingAccount_ReturnsNull()
        {
            using var ctx = CreateInMemoryContext(nameof(GetCustomerAsync_NonExistingAccount_ReturnsNull));

            var svc = new CustomerService(ctx);
            var result = await svc.GetCustomerAsync("NWSDB99999");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCustomerSummaryAsync_ExistingAccount_ReturnsSummary()
        {
            using var ctx = CreateInMemoryContext(nameof(GetCustomerSummaryAsync_ExistingAccount_ReturnsSummary));
            var customer = SampleCustomer();
            ctx.Customers.Add(customer);
            ctx.Bills.Add(new Bill
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
            });
            await ctx.SaveChangesAsync();

            var svc = new CustomerService(ctx);
            var result = await svc.GetCustomerSummaryAsync("NWSDB10001");

            Assert.NotNull(result);
            Assert.Equal("NWSDB10001", result.Customer.AccountNumber);
            Assert.Equal(1, result.NumberOfUnpaidBills);
            Assert.Equal(1500m, result.CurrentOutstandingAmount);
        }

        [Fact]
        public async Task GetCustomerSummaryAsync_NonExistingAccount_ReturnsNull()
        {
            using var ctx = CreateInMemoryContext(nameof(GetCustomerSummaryAsync_NonExistingAccount_ReturnsNull));

            var svc = new CustomerService(ctx);
            var result = await svc.GetCustomerSummaryAsync("NWSDB99999");

            Assert.Null(result);
        }
    }
}
