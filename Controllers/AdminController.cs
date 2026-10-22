using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NWSDB.API.Data;
using NWSDB.API.DTOs;
using NWSDB.API.DTOs.Admin;

namespace NWSDB.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly NWSDBDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(NWSDBDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ResponseDto<AdminStatsDto>>> GetStats()
        {
            _logger.LogInformation("Calculating NWSDB Admin dashboard statistics");

            var totalCustomers = await _context.Customers.CountAsync();
            var totalBills = await _context.Bills.CountAsync();
            var unpaidBills = await _context.Bills.CountAsync(b => b.Status == "Unpaid");
            var paidBills = await _context.Bills.CountAsync(b => b.Status == "Paid");
            var overdueBills = await _context.Bills.CountAsync(b => b.Status == "Overdue");
            var totalPayments = await _context.Payments.CountAsync();
            var totalRevenue = await _context.Payments
                .Where(p => p.Status == "Success")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;
            var totalOutstanding = await _context.Bills
                .Where(b => b.Status == "Unpaid" || b.Status == "Overdue")
                .SumAsync(b => (decimal?)b.BillAmount) ?? 0m;

            var stats = new AdminStatsDto
            {
                TotalCustomers = totalCustomers,
                TotalBills = totalBills,
                UnpaidBills = unpaidBills,
                PaidBills = paidBills,
                OverdueBills = overdueBills,
                TotalPayments = totalPayments,
                TotalRevenueCollected = totalRevenue,
                TotalOutstandingAmount = totalOutstanding
            };

            return Ok(new ResponseDto<AdminStatsDto>
            {
                Success = true,
                Message = "Admin statistics calculated successfully.",
                Data = stats
            });
        }
    }
}
