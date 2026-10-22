using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.Web.Models;
using NWSDB.Web.Services;

namespace NWSDB.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<AccountController> _logger;

        public AccountController(INwsdbApiClient apiClient, ILogger<AccountController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? accountNumber)
        {
            var acc = accountNumber ?? HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            acc = acc.Trim().ToUpper();

            HttpContext.Session.SetString("CurrentAccountNumber", acc);

            var summary = await _apiClient.GetCustomerSummaryAsync(acc);
            if (summary == null || summary.Customer == null)
            {
                ViewBag.ErrorMessage = $"Account '{acc}' was not found in NWSDB records.";
                ViewBag.RequestedAccount = acc;
                return View("NotFound");
            }

            return View(summary);
        }
    }
}
