using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWSDB.API.Data;
using NWSDB.API.DTOs.Customers;
using NWSDB.API.DTOs.Bills;
using NWSDB.API.DTOs.WaterUsage;
using NWSDB.API.DTOs.Payments;
using NWSDB.API.Interfaces;
using NWSDB.API.Models;

namespace NWSDB.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly NWSDBDbContext _context;

        public CustomerService(NWSDBDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.AccountNumber)
                .ToListAsync();

            return customers.Select(c => new CustomerDto
            {
                AccountNumber = c.AccountNumber,
                FullName = c.FullName,
                Address = c.Address,
                PhoneNumber = c.PhoneNumber,
                Email = c.Email
            });
        }

        public async Task<CustomerDto?> GetCustomerAsync(string accountNumber)
        {
            accountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty;
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (customer == null) return null;

            return new CustomerDto
            {
                AccountNumber = customer.AccountNumber,
                FullName = customer.FullName,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email
            };
        }

        public async Task<CustomerSummaryDto?> GetCustomerSummaryAsync(string accountNumber)
        {
            accountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty;
            var customer = await _context.Customers
                .Include(c => c.Bills)
                .Include(c => c.WaterUsages)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (customer == null) return null;

            var unpaidBills = customer.Bills.Where(b => b.Status == "Unpaid" || b.Status == "Overdue").ToList();
            var outstandingAmount = unpaidBills.Sum(b => b.BillAmount);
            var latestBill = customer.Bills.OrderByDescending(b => b.CreatedAt).FirstOrDefault();
            var currentUsage = customer.WaterUsages.OrderByDescending(u => u.RecordedAt).FirstOrDefault();
            
            var latestPayment = await _context.Payments
                .Where(p => p.AccountNumber == accountNumber)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();

            return new CustomerSummaryDto
            {
                Customer = new CustomerDto
                {
                    AccountNumber = customer.AccountNumber,
                    FullName = customer.FullName,
                    Address = customer.Address,
                    PhoneNumber = customer.PhoneNumber,
                    Email = customer.Email
                },
                CurrentOutstandingAmount = outstandingAmount,
                NumberOfUnpaidBills = unpaidBills.Count,
                LatestBill = latestBill == null ? null : new BillDto
                {
                    BillId = latestBill.BillId,
                    BillingMonth = latestBill.BillingMonth,
                    PreviousReading = latestBill.PreviousReading,
                    CurrentReading = latestBill.CurrentReading,
                    UnitsConsumed = latestBill.UnitsConsumed,
                    BillAmount = latestBill.BillAmount,
                    DueDate = latestBill.DueDate,
                    Status = latestBill.Status
                },
                CurrentWaterUsage = currentUsage == null ? null : new WaterUsageDto
                {
                    ReadingMonth = currentUsage.ReadingMonth,
                    PreviousReading = currentUsage.PreviousReading,
                    CurrentReading = currentUsage.CurrentReading,
                    UnitsConsumed = currentUsage.UnitsConsumed,
                    RecordedDate = currentUsage.RecordedAt
                },
                LatestPayment = latestPayment == null ? null : new PaymentDto
                {
                    PaymentId = latestPayment.PaymentId,
                    BillId = latestPayment.BillId,
                    Amount = latestPayment.Amount,
                    TransactionId = latestPayment.TransactionId,
                    PaymentMethod = latestPayment.PaymentMethod,
                    Status = latestPayment.Status,
                    PaymentDate = latestPayment.PaymentDate
                }
            };
        }

        public async Task<string> GetNextAccountNumberAsync()
        {
            var accounts = await _context.Customers
                .AsNoTracking()
                .Select(c => c.AccountNumber)
                .ToListAsync();

            int maxNum = 10000;
            foreach (var acc in accounts)
            {
                if (acc != null && acc.StartsWith("NWSDB", System.StringComparison.OrdinalIgnoreCase))
                {
                    var numPart = acc.Substring(5);
                    if (int.TryParse(numPart, out int parsed))
                    {
                        if (parsed > maxNum)
                        {
                            maxNum = parsed;
                        }
                    }
                }
            }

            return $"NWSDB{maxNum + 1}";
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
        {
            var accNum = dto.AccountNumber?.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(accNum))
            {
                accNum = await GetNextAccountNumberAsync();
            }

            var customer = new Customer
            {
                AccountNumber = accNum,
                FullName = dto.FullName.Trim(),
                Address = dto.Address.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Email = dto.Email.Trim(),
                CreatedAt = System.DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return new CustomerDto
            {
                AccountNumber = customer.AccountNumber,
                FullName = customer.FullName,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email
            };
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(string accountNumber, UpdateCustomerDto dto)
        {
            accountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);

            if (customer == null)
            {
                return null;
            }

            customer.FullName = dto.FullName.Trim();
            customer.Address = dto.Address.Trim();
            customer.PhoneNumber = dto.PhoneNumber.Trim();
            customer.Email = dto.Email.Trim();

            await _context.SaveChangesAsync();

            return new CustomerDto
            {
                AccountNumber = customer.AccountNumber,
                FullName = customer.FullName,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email
            };
        }
    }
}
