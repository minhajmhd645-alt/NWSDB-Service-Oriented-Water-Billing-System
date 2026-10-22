using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.Web.Models;
using NWSDB.Web.Services;

namespace NWSDB.Web.Controllers
{
    public class BillsController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<BillsController> _logger;

        public BillsController(INwsdbApiClient apiClient, ILogger<BillsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? accountNumber)
        {
            var acc = accountNumber ?? HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            acc = acc.Trim().ToUpper();

            HttpContext.Session.SetString("CurrentAccountNumber", acc);

            var bills = await _apiClient.GetBillsAsync(acc);
            var customer = await _apiClient.GetCustomerAsync(acc);

            var viewModel = new BillsListViewModel
            {
                AccountNumber = acc,
                CustomerName = customer?.FullName ?? acc,
                Bills = bills.ToList(),
                IsUnpaidOnly = false
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Unpaid(string? accountNumber)
        {
            var acc = accountNumber ?? HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            acc = acc.Trim().ToUpper();

            HttpContext.Session.SetString("CurrentAccountNumber", acc);

            var unpaidBills = await _apiClient.GetUnpaidBillsAsync(acc);
            var customer = await _apiClient.GetCustomerAsync(acc);

            var viewModel = new BillsListViewModel
            {
                AccountNumber = acc,
                CustomerName = customer?.FullName ?? acc,
                Bills = unpaidBills.ToList(),
                IsUnpaidOnly = true
            };

            return View(viewModel);
        }
    }
}
