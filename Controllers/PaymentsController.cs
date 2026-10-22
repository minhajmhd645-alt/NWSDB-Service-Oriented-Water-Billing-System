using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.Web.Models;
using NWSDB.Web.Services;

namespace NWSDB.Web.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(INwsdbApiClient apiClient, ILogger<PaymentsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> History(string? accountNumber)
        {
            var acc = accountNumber ?? HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            acc = acc.Trim().ToUpper();

            HttpContext.Session.SetString("CurrentAccountNumber", acc);

            var payments = await _apiClient.GetPaymentHistoryAsync(acc);
            var customer = await _apiClient.GetCustomerAsync(acc);

            ViewBag.AccountNumber = acc;
            ViewBag.CustomerName = customer?.FullName ?? acc;

            return View(payments.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int billId, string? accountNumber)
        {
            var acc = accountNumber ?? HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            acc = acc.Trim().ToUpper();

            HttpContext.Session.SetString("CurrentAccountNumber", acc);

            var bill = await _apiClient.GetBillAsync(billId);
            if (bill == null)
            {
                return NotFound("Bill not found.");
            }

            if (bill.Status == "Paid")
            {
                TempData["ErrorMessage"] = "This bill is already paid.";
                return RedirectToAction("Unpaid", "Bills", new { accountNumber = acc });
            }

            var customer = await _apiClient.GetCustomerAsync(acc);

            var viewModel = new PaymentFormViewModel
            {
                BillId = bill.BillId,
                AccountNumber = acc,
                CustomerName = customer?.FullName ?? acc,
                BillAmount = bill.BillAmount,
                BillingMonth = bill.BillingMonth,
                DueDate = bill.DueDate,
                PaymentMethod = "DemoBankAccount",
                BankAccountNumber = "DEMO-BANK-001"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(PaymentFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Re-fetch bill from API to enforce that customer cannot tamper with amount
            var bill = await _apiClient.GetBillAsync(model.BillId);
            if (bill == null)
            {
                model.ErrorMessage = "The bill could not be found.";
                return View(model);
            }

            if (bill.Status == "Paid")
            {
                model.ErrorMessage = "This bill is already paid.";
                return View(model);
            }

            var request = new PaymentRequestModel
            {
                BillId = bill.BillId,
                AccountNumber = model.AccountNumber,
                Amount = bill.BillAmount, // Lock to exact bill amount
                PaymentMethod = "DemoBankAccount",
                BankAccountNumber = model.BankAccountNumber.Trim()
            };

            var (success, payment, errorMessage) = await _apiClient.MakePaymentAsync(request);

            if (success && payment != null)
            {
                return RedirectToAction(nameof(Success), new { paymentId = payment.PaymentId, accountNumber = model.AccountNumber });
            }

            return RedirectToAction(nameof(Failure), new { billId = model.BillId, accountNumber = model.AccountNumber, amount = bill.BillAmount, errorMessage });
        }

        public async Task<IActionResult> Success(int paymentId, string? accountNumber)
        {
            var acc = accountNumber ?? HttpContext.Session.GetString("CurrentAccountNumber") ?? "NWSDB10001";
            acc = acc.Trim().ToUpper();

            var payment = await _apiClient.GetPaymentAsync(paymentId);
            if (payment == null)
            {
                return NotFound("Payment record not found.");
            }

            var viewModel = new PaymentReceiptViewModel
            {
                PaymentId = payment.PaymentId,
                BillId = payment.BillId,
                AccountNumber = acc,
                Amount = payment.Amount,
                TransactionId = payment.TransactionId,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate
            };

            return View(viewModel);
        }

        public IActionResult Failure(int billId, string accountNumber, decimal amount, string errorMessage)
        {
            var viewModel = new PaymentFailureViewModel
            {
                BillId = billId,
                AccountNumber = accountNumber,
                Amount = amount,
                ErrorMessage = string.IsNullOrWhiteSpace(errorMessage)
                    ? "Payment could not be completed. Please check your account details and try again."
                    : errorMessage
            };

            return View(viewModel);
        }
    }
}
