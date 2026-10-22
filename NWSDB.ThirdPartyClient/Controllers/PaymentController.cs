using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NWSDB.ThirdPartyClient.Models;
using NWSDB.ThirdPartyClient.Services;

namespace NWSDB.ThirdPartyClient.Controllers
{
    public class PaymentController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentController(INwsdbApiClient apiClient, ILogger<PaymentController> logger, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _configuration = configuration;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helper
        // ─────────────────────────────────────────────────────────────────────
        private string GetNwsdbWebBaseUrl() =>
            _configuration["NwsdbWebSettings:BaseUrl"] ?? "https://localhost:7050";

        // ─────────────────────────────────────────────────────────────────────
        // Payment History (unchanged)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> History(string? accountNumber, CancellationToken cancellationToken)
        {
            var model = new CustomerLookupViewModel();

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return View(model);
            }

            accountNumber = accountNumber.Trim().ToUpper();
            model.AccountNumber = accountNumber;

            _logger.LogInformation("Retrieving payment history for account: {AccountNumber}", accountNumber);

            var customerRes = await _apiClient.GetCustomerAsync(accountNumber, cancellationToken);
            if (customerRes != null && customerRes.Success && customerRes.Data != null)
            {
                model.Customer = customerRes.Data;
            }

            var paymentsRes = await _apiClient.GetPaymentsAsync(accountNumber, cancellationToken);

            if (paymentsRes == null || (!paymentsRes.Success && paymentsRes.Message.Contains("unavailable", System.StringComparison.OrdinalIgnoreCase)))
            {
                model.IsServiceUnavailable = true;
                model.ErrorMessage = "NWSDB services are temporarily unavailable. Please try again later.";
                return View(model);
            }

            if (!paymentsRes.Success && (customerRes == null || !customerRes.Success))
            {
                model.IsNotFound = true;
                model.ErrorMessage = "Customer account was not found.";
                return View(model);
            }

            model.PaymentHistory = paymentsRes.Data ?? new List<PaymentDto>();
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // NEW STEP 1 – Payment Channel Selection
        // Entry point from the "Pay Now" button in Unpaid Bills.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Renders the "Choose Your Payment Channel" page.
        /// The customer selects a channel before reaching the Pay form.
        /// No external APIs are called here.
        /// </summary>
        [HttpGet]
        public IActionResult SelectChannel(int billId, string accountNumber, decimal amount, string? billingMonth)
        {
            var model = new PaymentChannelViewModel
            {
                BillId = billId,
                AccountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty,
                Amount = amount,
                BillingMonth = billingMonth ?? "Current Month",
                NwsdbWebBaseUrl = GetNwsdbWebBaseUrl()
            };

            return View(model);
        }

        /// <summary>
        /// Handles the channel-selection form POST.
        /// Routes to the PartnerInfo confirmation page.
        /// No real external-partner APIs are called – channels are
        /// SOC demonstration representations only.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmChannel(PaymentChannelViewModel model)
        {
            // Re-read from config (not stored as hidden field to keep form small)
            model.NwsdbWebBaseUrl = GetNwsdbWebBaseUrl();

            _logger.LogInformation(
                "Payment channel selected: {Channel} | BillId: {BillId} | Account: {Account}",
                model.SelectedChannel, model.BillId, model.AccountNumber);

            return View("PartnerInfo", model);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Pay form (unchanged logic; channelName is optional display-only)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Pay(int billId, string accountNumber, decimal amount, string? billingMonth, string? channelName)
        {
            var model = new PayBillViewModel
            {
                BillId = billId,
                AccountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty,
                Amount = amount,
                BillingMonth = billingMonth ?? "Current Month",
                PaymentMethod = "DemoBankAccount",
                BankAccountNumber = "DEMO-BANK-001",
                ChannelName = channelName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPay(PayBillViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View("Pay", model);
            }

            _logger.LogInformation("Initiating third-party payment for BillId: {BillId}, Account: {AccountNumber}, Amount: {Amount}",
                model.BillId, model.AccountNumber, model.Amount);

            var dto = new PaymentRequestDto
            {
                BillId = model.BillId,
                AccountNumber = model.AccountNumber,
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                BankAccountNumber = model.BankAccountNumber
            };

            var response = await _apiClient.ProcessPaymentAsync(dto, cancellationToken);

            if (response != null && response.Success && response.Data != null)
            {
                _logger.LogInformation("Payment successful for BillId: {BillId}, PaymentId: {PaymentId}", model.BillId, response.Data.PaymentId);

                return RedirectToAction(nameof(Success), new {
                    paymentId = response.Data.PaymentId,
                    accountNumber = model.AccountNumber,
                    billId = model.BillId
                });
            }

            string failureMessage = response?.Message ?? "Payment processing failed. Please try again later.";
            _logger.LogWarning("Payment rejected for BillId: {BillId}. Reason: {Reason}", model.BillId, failureMessage);

            return RedirectToAction(nameof(Failure), new {
                billId = model.BillId,
                accountNumber = model.AccountNumber,
                amount = model.Amount,
                message = failureMessage,
                paymentMethod = model.PaymentMethod,
                bankAccountNumber = model.BankAccountNumber
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Success / Failure (unchanged)
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Success(int paymentId, string accountNumber, int billId, CancellationToken cancellationToken)
        {
            var model = new PaymentResultViewModel
            {
                IsSuccess = true,
                BillId = billId,
                AccountNumber = accountNumber,
                Message = "Payment completed successfully."
            };

            var paymentsRes = await _apiClient.GetPaymentsAsync(accountNumber, cancellationToken);
            if (paymentsRes != null && paymentsRes.Success && paymentsRes.Data != null)
            {
                var payment = System.Linq.Enumerable.FirstOrDefault(paymentsRes.Data, p => p.PaymentId == paymentId || p.BillId == billId);
                if (payment != null)
                {
                    model.Payment = payment;
                    model.AttemptedAmount = payment.Amount;
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Failure(int billId, string accountNumber, decimal amount, string message, string? paymentMethod, string? bankAccountNumber)
        {
            var model = new PaymentResultViewModel
            {
                IsSuccess = false,
                BillId = billId,
                AccountNumber = accountNumber,
                AttemptedAmount = amount,
                FailureReason = message,
                PaymentMethod = paymentMethod ?? "DemoBankAccount",
                BankAccountNumber = bankAccountNumber ?? "N/A",
                Message = "Payment processing was unsuccessful."
            };

            return View(model);
        }
    }
}
