using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NWSDB.API.Data;
using NWSDB.API.DTOs.Bills;
using NWSDB.API.Interfaces;

namespace NWSDB.API.Services
{
    public class BillService : IBillService
    {
        private readonly NWSDBDbContext _context;

        public BillService(NWSDBDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BillDto>> GetAllBillsAsync()
        {
            var bills = await _context.Bills
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return bills.Select(b => new BillDto
            {
                BillId = b.BillId,
                AccountNumber = b.AccountNumber,
                BillingMonth = b.BillingMonth,
                PreviousReading = b.PreviousReading,
                CurrentReading = b.CurrentReading,
                UnitsConsumed = b.UnitsConsumed,
                BillAmount = b.BillAmount,
                DueDate = b.DueDate,
                Status = b.Status
            });
        }

        public async Task<IEnumerable<BillDto>> GetBillsAsync(string accountNumber)
        {
            var bills = await _context.Bills
                .Where(b => b.AccountNumber == accountNumber)
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return bills.Select(b => new BillDto
            {
                BillId = b.BillId,
                AccountNumber = b.AccountNumber,
                BillingMonth = b.BillingMonth,
                PreviousReading = b.PreviousReading,
                CurrentReading = b.CurrentReading,
                UnitsConsumed = b.UnitsConsumed,
                BillAmount = b.BillAmount,
                DueDate = b.DueDate,
                Status = b.Status
            });
        }

        public async Task<IEnumerable<BillDto>> GetUnpaidBillsAsync(string accountNumber)
        {
            var bills = await _context.Bills
                .Where(b => b.AccountNumber == accountNumber && (b.Status == "Unpaid" || b.Status == "Overdue"))
                .OrderBy(b => b.DueDate)
                .AsNoTracking()
                .ToListAsync();

            return bills.Select(b => new BillDto
            {
                BillId = b.BillId,
                AccountNumber = b.AccountNumber,
                BillingMonth = b.BillingMonth,
                PreviousReading = b.PreviousReading,
                CurrentReading = b.CurrentReading,
                UnitsConsumed = b.UnitsConsumed,
                BillAmount = b.BillAmount,
                DueDate = b.DueDate,
                Status = b.Status
            });
        }

        public async Task<BillDto?> GetBillAsync(int billId)
        {
            var b = await _context.Bills
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BillId == billId);

            if (b == null) return null;

            return new BillDto
            {
                BillId = b.BillId,
                AccountNumber = b.AccountNumber,
                BillingMonth = b.BillingMonth,
                PreviousReading = b.PreviousReading,
                CurrentReading = b.CurrentReading,
                UnitsConsumed = b.UnitsConsumed,
                BillAmount = b.BillAmount,
                DueDate = b.DueDate,
                Status = b.Status
            };
        }

        public async Task<BillDto> CreateBillAsync(CreateBillDto dto)
        {
            var accountNumber = dto.AccountNumber?.Trim().ToUpper() ?? string.Empty;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.AccountNumber == accountNumber);
            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer account '{accountNumber}' was not found.");
            }

            int units = Math.Max(0, dto.CurrentReading - dto.PreviousReading);
            decimal amount = dto.BillAmount > 0 ? dto.BillAmount : (units > 0 ? units * 30.0m : 500.0m);

            var bill = new Models.Bill
            {
                AccountNumber = accountNumber,
                BillingMonth = dto.BillingMonth.Trim(),
                PreviousReading = dto.PreviousReading,
                CurrentReading = dto.CurrentReading,
                UnitsConsumed = units,
                BillAmount = amount,
                DueDate = dto.DueDate == default ? System.DateTime.UtcNow.AddDays(14) : dto.DueDate,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Unpaid" : dto.Status.Trim(),
                CreatedAt = System.DateTime.UtcNow
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return new BillDto
            {
                BillId = bill.BillId,
                AccountNumber = bill.AccountNumber,
                BillingMonth = bill.BillingMonth,
                PreviousReading = bill.PreviousReading,
                CurrentReading = bill.CurrentReading,
                UnitsConsumed = bill.UnitsConsumed,
                BillAmount = bill.BillAmount,
                DueDate = bill.DueDate,
                Status = bill.Status
            };
        }

        public async Task<BillDto?> UpdateBillAsync(int billId, UpdateBillDto dto)
        {
            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillId == billId);
            if (bill == null) return null;

            int units = Math.Max(0, dto.CurrentReading - dto.PreviousReading);

            bill.BillingMonth = dto.BillingMonth.Trim();
            bill.PreviousReading = dto.PreviousReading;
            bill.CurrentReading = dto.CurrentReading;
            bill.UnitsConsumed = units;
            bill.BillAmount = dto.BillAmount;
            bill.DueDate = dto.DueDate;
            bill.Status = dto.Status.Trim();

            await _context.SaveChangesAsync();

            return new BillDto
            {
                BillId = bill.BillId,
                AccountNumber = bill.AccountNumber,
                BillingMonth = bill.BillingMonth,
                PreviousReading = bill.PreviousReading,
                CurrentReading = bill.CurrentReading,
                UnitsConsumed = bill.UnitsConsumed,
                BillAmount = bill.BillAmount,
                DueDate = bill.DueDate,
                Status = bill.Status
            };
        }
    }
}
