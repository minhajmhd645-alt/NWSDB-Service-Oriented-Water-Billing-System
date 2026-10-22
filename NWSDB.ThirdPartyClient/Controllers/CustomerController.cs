using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.ThirdPartyClient.Models;
using NWSDB.ThirdPartyClient.Services;

namespace NWSDB.ThirdPartyClient.Controllers
{
    public class CustomerController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(INwsdbApiClient apiClient, ILogger<CustomerController> logger)
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

            _logger.LogInformation("Processing customer search for account: {AccountNumber}", accountNumber);

            var customerRes = await _apiClient.GetCustomerAsync(accountNumber, cancellationToken);

            if (customerRes == null)
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "Unable to connect to the NWSDB service. Please make sure the NWSDB API is running and try again.";
                return View("Index", model);
            }

            if (!customerRes.Success || customerRes.Data == null)
            {
                if (customerRes.Message.Contains("unavailable", System.StringComparison.OrdinalIgnoreCase))
                {
                    model.IsServiceUnavailable = true;
                    model.ErrorMessage = "Unable to connect to the NWSDB service. Please make sure the NWSDB API is running and try again.";
                }
                else
                {
                    model.IsNotFound = true;
                    model.ErrorMessage = "Customer account not found.";
                }
                return View("Index", model);
            }

            model.Customer = customerRes.Data;

            // Fetch summary
            var summaryRes = await _apiClient.GetCustomerSummaryAsync(accountNumber, cancellationToken);
            if (summaryRes != null && summaryRes.Success)
            {
                model.Summary = summaryRes.Data;
            }

            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? accountNumber, CancellationToken cancellationToken)
        {
            return await Index(accountNumber, cancellationToken);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string accountNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return RedirectToAction(nameof(Index));
            }

            accountNumber = accountNumber.Trim().ToUpper();

            _logger.LogInformation("Retrieving customer details and summary for account: {AccountNumber}", accountNumber);

            var model = new CustomerLookupViewModel
            {
                AccountNumber = accountNumber
            };

            var summaryRes = await _apiClient.GetCustomerSummaryAsync(accountNumber, cancellationToken);

            if (summaryRes == null || (!summaryRes.Success && summaryRes.Message.Contains("unavailable", System.StringComparison.OrdinalIgnoreCase)))
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "NWSDB services are temporarily unavailable. Please try again later.";
                return View("Index", model);
            }

            if (!summaryRes.Success || summaryRes.Data == null)
            {
                model.IsNotFound = true;
                model.ErrorMessage = "Customer account was not found.";
                return View("Index", model);
            }

            model.Summary = summaryRes.Data;
            model.Customer = summaryRes.Data.Customer;

            // Also load unpaid bills for summary overview
            var unpaidRes = await _apiClient.GetUnpaidBillsAsync(accountNumber, cancellationToken);
            if (unpaidRes != null && unpaidRes.Success && unpaidRes.Data != null)
            {
                model.UnpaidBills = unpaidRes.Data;
            }

            return View(model);
        }
    }
}
