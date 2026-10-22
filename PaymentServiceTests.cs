using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NWSDB.API.Data;
using NWSDB.API.DTOs.Payments;
using NWSDB.API.Exceptions;
using NWSDB.API.Interfaces;
using NWSDB.API.Models;
using NWSDB.API.Services;
using Xunit;

namespace NWSDB.API.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IBankApiClient> _mockBankApiClient = new();
        private readonly Mock<ILogger<PaymentService>> _mockLogger = new();

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

        private static Customer AnotherCustomer() => new Customer
        {
            CustomerId = 2,
            AccountNumber = "NWSDB10002",
            FullName = "Nimal Silva",
            Address = "Kandy",
            PhoneNumber = "0722222222",
            Email = "nimal@example.com"
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
        public async Task CreatePaymentAsync_ValidPayment_Succeeds()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_ValidPayment_Succeeds));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            await ctx.SaveChangesAsync();

            _mockBankApiClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
                .ReturnsAsync(new BankPaymentResult
                {
                    TransactionId = "BNK-TEST-1001",
                    Status = "Success",
                    Amount = 1500,
                    Currency = "LKR",
                    Message = "Payment processed successfully."
                });

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "DemoCard"
            };

            var result = await svc.CreatePaymentAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Success", result.Status);
            Assert.Equal("BNK-TEST-1001", result.TransactionId);
        }

        [Fact]
        public async Task CreatePaymentAsync_NonExistingCustomer_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_NonExistingCustomer_ThrowsArgumentException));

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB99999",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "DemoCard"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
        }

        [Fact]
        public async Task CreatePaymentAsync_NonExistingBill_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_NonExistingBill_ThrowsArgumentException));
            ctx.Customers.Add(SampleCustomer());
            await ctx.SaveChangesAsync();

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 9999,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "DemoCard"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
        }

        [Fact]
        public async Task CreatePaymentAsync_BillBelongsToAnotherCustomer_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_BillBelongsToAnotherCustomer_ThrowsArgumentException));
            ctx.Customers.Add(SampleCustomer());
            ctx.Customers.Add(AnotherCustomer());
            ctx.Bills.Add(UnpaidBill()); // Belongs to NWSDB10001
            await ctx.SaveChangesAsync();

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB10002", // Different account
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "DemoCard"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
            Assert.Contains("does not belong", ex.Message);
        }

        [Fact]
        public async Task CreatePaymentAsync_AlreadyPaidBill_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_AlreadyPaidBill_ThrowsArgumentException));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(PaidBill());
            await ctx.SaveChangesAsync();

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 2,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "DemoCard"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
            Assert.Contains("already paid", ex.Message);
        }

        [Fact]
        public async Task CreatePaymentAsync_ZeroAmount_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_ZeroAmount_ThrowsArgumentException));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            await ctx.SaveChangesAsync();

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 0,
                PaymentMethod = "DemoCard"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
            Assert.Contains("greater than zero", ex.Message);
        }

        [Fact]
        public async Task CreatePaymentAsync_InvalidPaymentMethod_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_InvalidPaymentMethod_ThrowsArgumentException));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            await ctx.SaveChangesAsync();

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "Visa"  // Invalid - not DemoCard or DemoBankAccount
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
            Assert.Contains("Invalid payment method", ex.Message);
        }

        [Fact]
        public async Task CreatePaymentAsync_MissingBankAccountNumber_ThrowsArgumentException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_MissingBankAccountNumber_ThrowsArgumentException));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            await ctx.SaveChangesAsync();

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "",
                Amount = 1500,
                PaymentMethod = "DemoBankAccount"
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => svc.CreatePaymentAsync(request));
            Assert.Contains("Bank account number is required", ex.Message);
        }

        [Fact]
        public async Task CreatePaymentAsync_BankApiFails_ThrowsBankApiException()
        {
            using var ctx = CreateInMemoryContext(nameof(CreatePaymentAsync_BankApiFails_ThrowsBankApiException));
            ctx.Customers.Add(SampleCustomer());
            ctx.Bills.Add(UnpaidBill());
            await ctx.SaveChangesAsync();

            _mockBankApiClient
                .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>()))
                .ThrowsAsync(new BankApiException("Insufficient balance on account 'DEMO-BANK-001'.", HttpStatusCode.BadRequest));

            var svc = new PaymentService(ctx, _mockBankApiClient.Object, _mockLogger.Object);
            var request = new PaymentRequestDto
            {
                BillId = 1,
                AccountNumber = "NWSDB10001",
                BankAccountNumber = "DEMO-BANK-001",
                Amount = 1500,
                PaymentMethod = "DemoBankAccount"
            };

            var ex = await Assert.ThrowsAsync<BankApiException>(() => svc.CreatePaymentAsync(request));
            Assert.Contains("Insufficient balance", ex.Message);
            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);

            // Verify Bill was NOT marked as Paid
            var bill = await ctx.Bills.FindAsync(1);
            Assert.Equal("Unpaid", bill!.Status);
        }
    }
}
