using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.ThirdPartyClient.Models;
using NWSDB.ThirdPartyClient.Services;

namespace NWSDB.ThirdPartyClient.Controllers
{
    public class UsageController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<UsageController> _logger;

        public UsageController(INwsdbApiClient apiClient, ILogger<UsageController> logger)
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

            _logger.LogInformation("Retrieving water usage for account: {AccountNumber}", accountNumber);

            // Fetch customer profile details
            var customerRes = await _apiClient.GetCustomerAsync(accountNumber, cancellationToken);
            if (customerRes != null && customerRes.Success && customerRes.Data != null)
            {
                model.Customer = customerRes.Data;
            }
            else if (customerRes != null && !customerRes.Success && (customerRes.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase) || customerRes.Message.Contains("connect", StringComparison.OrdinalIgnoreCase)))
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "Unable to connect to the NWSDB service. Please try again later.";
                return View(model);
            }

            // Fetch current water usage
            var usageRes = await _apiClient.GetCurrentUsageAsync(accountNumber, cancellationToken);

            if (usageRes == null)
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "Unable to connect to the NWSDB service. Please try again later.";
                return View(model);
            }

            if (!usageRes.Success)
            {
                if (usageRes.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase))
                {
                    model.ErrorMessage = "NWSDB service request timed out. Please try again later.";
                    return View(model);
                }

                if (usageRes.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase) || usageRes.Message.Contains("connect", StringComparison.OrdinalIgnoreCase))
                {
                    model.IsServiceUnavailable = true;
                    model.ErrorMessage = "Unable to connect to the NWSDB service. Please try again later.";
                    return View(model);
                }

                if (customerRes == null || !customerRes.Success)
                {
                    model.IsNotFound = true;
                    model.ErrorMessage = "Customer account not found.";
                    return View(model);
                }

                // Customer exists, but water usage record is not found (HTTP 404)
                model.ErrorMessage = "Water usage information was not found for this account.";
                return View(model);
            }

            model.CurrentUsage = usageRes.Data;
            return View(model);
        }
    }
}
