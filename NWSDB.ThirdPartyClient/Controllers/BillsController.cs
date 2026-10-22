using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.ThirdPartyClient.Models;
using NWSDB.ThirdPartyClient.Services;

namespace NWSDB.ThirdPartyClient.Controllers
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

        [HttpGet]
        public async Task<IActionResult> Index(string? accountNumber, CancellationToken cancellationToken)
        {
            var model = new CustomerLookupViewModel();

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return View(model);
            }

            accountNumber = accountNumber.Trim().ToUpper();
            model.AccountNumber = accountNumber;

            _logger.LogInformation("Retrieving bill history for account: {AccountNumber}", accountNumber);

            var customerRes = await _apiClient.GetCustomerAsync(accountNumber, cancellationToken);
            if (customerRes != null && customerRes.Success && customerRes.Data != null)
            {
                model.Customer = customerRes.Data;
            }

            var billsRes = await _apiClient.GetBillsAsync(accountNumber, cancellationToken);

            if (billsRes == null || (!billsRes.Success && billsRes.Message.Contains("unavailable", System.StringComparison.OrdinalIgnoreCase)))
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "NWSDB services are temporarily unavailable. Please try again later.";
                return View(model);
            }

            if (!billsRes.Success)
            {
                if (customerRes == null || !customerRes.Success)
                {
                    model.IsNotFound = true;
                    model.ErrorMessage = "Customer account was not found.";
                }
                else
                {
                    model.ErrorMessage = billsRes.Message;
                }
                return View(model);
            }

            model.Bills = billsRes.Data ?? new List<BillDto>();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Unpaid(string? accountNumber, CancellationToken cancellationToken)
        {
            var model = new CustomerLookupViewModel();

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return View(model);
            }

            accountNumber = accountNumber.Trim().ToUpper();
            model.AccountNumber = accountNumber;

            _logger.LogInformation("Retrieving unpaid bills for account: {AccountNumber}", accountNumber);

            var customerRes = await _apiClient.GetCustomerAsync(accountNumber, cancellationToken);
            if (customerRes != null && customerRes.Success && customerRes.Data != null)
            {
                model.Customer = customerRes.Data;
            }

            var unpaidRes = await _apiClient.GetUnpaidBillsAsync(accountNumber, cancellationToken);

            if (unpaidRes == null || (!unpaidRes.Success && unpaidRes.Message.Contains("unavailable", System.StringComparison.OrdinalIgnoreCase)))
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "NWSDB services are temporarily unavailable. Please try again later.";
                return View(model);
            }

            if (!unpaidRes.Success && (customerRes == null || !customerRes.Success))
            {
                model.IsNotFound = true;
                model.ErrorMessage = "Customer account was not found.";
                return View(model);
            }

            model.UnpaidBills = unpaidRes.Data ?? new List<BillDto>();
            return View(model);
        }
    }
}
