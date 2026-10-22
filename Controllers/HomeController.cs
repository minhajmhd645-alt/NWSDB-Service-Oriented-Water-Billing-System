using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.Web.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using NWSDB.Web.Services;

namespace NWSDB.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(INwsdbApiClient apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var currentAcc = HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            var summary = await _apiClient.GetCustomerSummaryAsync(currentAcc);

            ViewBag.CurrentAccountNumber = currentAcc;
            return View(summary);
        }

        [HttpPost]
        public IActionResult SelectAccount(AccountLookupViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.AccountNumber))
            {
                return RedirectToAction(nameof(Index));
            }

            var acc = model.AccountNumber.Trim().ToUpper();
            HttpContext.Session.SetString("CurrentAccountNumber", acc);
            return RedirectToAction("Index", "Account", new { accountNumber = acc });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
