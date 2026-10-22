using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NWSDB.Web.Models;
using NWSDB.Web.Services;

namespace NWSDB.Web.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly INwsdbApiClient _apiClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(INwsdbApiClient apiClient, ILogger<AdminController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: /Admin or /Admin/Index
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Loading Admin Dashboard");

            var statsTask = _apiClient.GetAdminStatsAsync(cancellationToken);
            var customersTask = _apiClient.GetAllCustomersAsync(cancellationToken);
            var billsTask = _apiClient.GetAllBillsAsync(cancellationToken);
            var paymentsTask = _apiClient.GetAllPaymentsAsync(cancellationToken);

            await Task.WhenAll(statsTask, customersTask, billsTask, paymentsTask);

            var stats = await statsTask ?? new AdminStatsModel();
            var customers = await customersTask;
            var bills = await billsTask;
            var payments = await paymentsTask;

            var model = new AdminDashboardViewModel
            {
                Stats = stats,
                RecentCustomers = customers.Take(5),
                RecentBills = bills.Take(5),
                RecentPayments = payments.Take(5)
            };

            return View(model);
        }

        // GET: /Admin/Customers
        [HttpGet("Customers")]
        public async Task<IActionResult> Customers(string? searchTerm, string? filterStatus, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving customers for Admin Customer Management");
            var customers = await _apiClient.GetAllCustomersAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                customers = customers.Where(c =>
                    (c.AccountNumber != null && c.AccountNumber.ToLower().Contains(term)) ||
                    (c.FullName != null && c.FullName.ToLower().Contains(term)) ||
                    (c.Address != null && c.Address.ToLower().Contains(term)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term))
                );
            }

            var viewModel = new CustomerListViewModel
            {
                Customers = customers,
                SearchTerm = searchTerm ?? string.Empty,
                FilterStatus = filterStatus ?? "All"
            };

            return View(viewModel);
        }

        // GET: /Admin/Customers/Create
        [HttpGet("Customers/Create")]
        public async Task<IActionResult> CreateCustomer(CancellationToken cancellationToken)
        {
            var nextAccount = await _apiClient.GetNextAccountNumberAsync(cancellationToken);
            var model = new AdminCreateCustomerViewModel
            {
                AccountNumber = string.IsNullOrWhiteSpace(nextAccount) ? "NWSDB10011" : nextAccount
            };

            return View(model);
        }

        // POST: /Admin/Customers/Create
        [HttpPost("Customers/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(AdminCreateCustomerViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new CreateCustomerRequestModel
            {
                AccountNumber = model.AccountNumber?.Trim().ToUpper() ?? string.Empty,
                FullName = model.FullName?.Trim() ?? string.Empty,
                Address = model.Address?.Trim() ?? string.Empty,
                PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty,
                Email = model.Email?.Trim() ?? string.Empty
            };

            var result = await _apiClient.CreateCustomerAsync(request, cancellationToken);

            if (result.Success && result.Customer != null)
            {
                var generatedAccount = result.Customer.AccountNumber;

                // Optionally create initial bill if specified
                if (model.InitialBillAmount > 0)
                {
                    var initialBill = new CreateBillRequestModel
                    {
                        AccountNumber = generatedAccount,
                        BillingMonth = DateTime.UtcNow.ToString("MMM-yyyy"),
                        PreviousReading = 0,
                        CurrentReading = Convert.ToInt32(model.InitialBillAmount / 30.0m),
                        BillAmount = model.InitialBillAmount,
                        DueDate = DateTime.UtcNow.AddDays(14),
                        Status = "Unpaid"
                    };
                    await _apiClient.CreateBillAsync(initialBill, cancellationToken);
                }

                TempData["SuccessMessage"] = $"Customer '{request.FullName}' was created successfully with Account Number: {generatedAccount}.";
                return RedirectToAction(nameof(Customers));
            }

            if (result.StatusCode == 409)
            {
                ModelState.AddModelError("AccountNumber", "Customer account number already exists.");
            }
            else
            {
                ViewBag.ErrorMessage = result.ErrorMessage;
            }

            return View(model);
        }

        // GET: /Admin/Customers/Edit/{accountNumber}
        [HttpGet("Customers/Edit/{accountNumber}")]
        public async Task<IActionResult> EditCustomer(string accountNumber, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return RedirectToAction(nameof(Customers));
            }

            accountNumber = accountNumber.Trim().ToUpper();
            var customer = await _apiClient.GetCustomerAsync(accountNumber, cancellationToken);

            if (customer == null)
            {
                TempData["ErrorMessage"] = $"Customer account '{accountNumber}' was not found.";
                return RedirectToAction(nameof(Customers));
            }

            var viewModel = new AdminEditCustomerViewModel
            {
                AccountNumber = customer.AccountNumber,
                FullName = customer.FullName,
                Address = customer.Address,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                AccountStatus = "Active"
            };

            return View(viewModel);
        }

        // POST: /Admin/Customers/Edit/{accountNumber}
        [HttpPost("Customers/Edit/{accountNumber}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(string accountNumber, AdminEditCustomerViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            accountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty;
            model.AccountNumber = accountNumber;

            var request = new UpdateCustomerRequestModel
            {
                FullName = model.FullName?.Trim() ?? string.Empty,
                Address = model.Address?.Trim() ?? string.Empty,
                PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty,
                Email = model.Email?.Trim() ?? string.Empty
            };

            var result = await _apiClient.UpdateCustomerAsync(accountNumber, request, cancellationToken);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Customer account '{accountNumber}' updated successfully.";
                return RedirectToAction(nameof(Customers));
            }

            if (result.StatusCode == 404)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(Customers));
            }

            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(model);
        }

        // GET: /Admin/Bills
        [HttpGet("Bills")]
        public async Task<IActionResult> Bills(string? searchAccount, string? statusFilter, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving bills for Admin Bill Management");
            var bills = await _apiClient.GetAllBillsAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(searchAccount))
            {
                var term = searchAccount.Trim().ToLower();
                bills = bills.Where(b =>
                    (b.AccountNumber != null && b.AccountNumber.ToLower().Contains(term)) ||
                    b.BillId.ToString().Contains(term)
                );
            }

            if (!string.IsNullOrWhiteSpace(statusFilter) && !statusFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                bills = bills.Where(b => b.Status != null && b.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            var viewModel = new BillListViewModel
            {
                Bills = bills,
                SearchAccount = searchAccount ?? string.Empty,
                StatusFilter = statusFilter ?? "All"
            };

            return View(viewModel);
        }

        // GET: /Admin/Bills/Create
        [HttpGet("Bills/Create")]
        public IActionResult CreateBill(string? accountNumber)
        {
            var model = new CreateBillViewModel
            {
                AccountNumber = accountNumber?.Trim().ToUpper() ?? string.Empty,
                BillingMonth = DateTime.UtcNow.ToString("MMM-yyyy"),
                DueDate = DateTime.UtcNow.AddDays(14)
            };
            return View(model);
        }

        // POST: /Admin/Bills/Create
        [HttpPost("Bills/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBill(CreateBillViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new CreateBillRequestModel
            {
                AccountNumber = model.AccountNumber?.Trim().ToUpper() ?? string.Empty,
                BillingMonth = model.BillingMonth?.Trim() ?? string.Empty,
                PreviousReading = model.PreviousReading,
                CurrentReading = model.CurrentReading,
                BillAmount = model.BillAmount,
                DueDate = model.DueDate,
                Status = model.Status ?? "Unpaid"
            };

            var result = await _apiClient.CreateBillAsync(request, cancellationToken);

            if (result.Success && result.Bill != null)
            {
                TempData["SuccessMessage"] = $"Bill #{result.Bill.BillId} for Account '{result.Bill.AccountNumber}' created successfully.";
                return RedirectToAction(nameof(Bills));
            }

            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(model);
        }

        // GET: /Admin/Bills/Details/{billId}
        [HttpGet("Bills/Details/{billId}")]
        public async Task<IActionResult> BillDetails(int billId, CancellationToken cancellationToken)
        {
            var bill = await _apiClient.GetBillAsync(billId, cancellationToken);
            if (bill == null)
            {
                TempData["ErrorMessage"] = $"Bill #{billId} was not found.";
                return RedirectToAction(nameof(Bills));
            }

            var customer = await _apiClient.GetCustomerAsync(bill.AccountNumber, cancellationToken);
            ViewBag.Customer = customer;

            return View(bill);
        }

        // GET: /Admin/Bills/Edit/{billId}
        [HttpGet("Bills/Edit/{billId}")]
        public async Task<IActionResult> EditBill(int billId, CancellationToken cancellationToken)
        {
            var bill = await _apiClient.GetBillAsync(billId, cancellationToken);
            if (bill == null)
            {
                TempData["ErrorMessage"] = $"Bill #{billId} was not found.";
                return RedirectToAction(nameof(Bills));
            }

            var model = new EditBillViewModel
            {
                BillId = bill.BillId,
                AccountNumber = bill.AccountNumber,
                BillingMonth = bill.BillingMonth,
                PreviousReading = bill.PreviousReading,
                CurrentReading = bill.CurrentReading,
                BillAmount = bill.BillAmount,
                DueDate = bill.DueDate,
                Status = bill.Status
            };

            return View(model);
        }

        // POST: /Admin/Bills/Edit/{billId}
        [HttpPost("Bills/Edit/{billId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBill(int billId, EditBillViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var request = new UpdateBillRequestModel
            {
                BillingMonth = model.BillingMonth?.Trim() ?? string.Empty,
                PreviousReading = model.PreviousReading,
                CurrentReading = model.CurrentReading,
                BillAmount = model.BillAmount,
                DueDate = model.DueDate,
                Status = model.Status ?? "Unpaid"
            };

            var result = await _apiClient.UpdateBillAsync(billId, request, cancellationToken);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Bill #{billId} updated successfully.";
                return RedirectToAction(nameof(Bills));
            }

            ViewBag.ErrorMessage = result.ErrorMessage;
            return View(model);
        }

        // GET: /Admin/Payments
        [HttpGet("Payments")]
        public async Task<IActionResult> Payments(string? searchAccount, string? methodFilter, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving payments for Admin Payment Management");
            var payments = await _apiClient.GetAllPaymentsAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(searchAccount))
            {
                var term = searchAccount.Trim().ToLower();
                payments = payments.Where(p =>
                    (p.AccountNumber != null && p.AccountNumber.ToLower().Contains(term)) ||
                    (p.TransactionId != null && p.TransactionId.ToLower().Contains(term)) ||
                    p.BillId.ToString().Contains(term)
                );
            }

            if (!string.IsNullOrWhiteSpace(methodFilter) && !methodFilter.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                payments = payments.Where(p => p.PaymentMethod != null && p.PaymentMethod.Equals(methodFilter, StringComparison.OrdinalIgnoreCase));
            }

            var viewModel = new PaymentListViewModel
            {
                Payments = payments,
                SearchAccount = searchAccount ?? string.Empty,
                MethodFilter = methodFilter ?? "All"
            };

            return View(viewModel);
        }

        // GET / POST: /Admin/PortalView/{accountNumber}
        [HttpGet("PortalView/{accountNumber}")]
        [HttpPost("PortalView/{accountNumber}")]
        public IActionResult PortalView(string accountNumber)
        {
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                accountNumber = accountNumber.Trim().ToUpper();
                Response.Cookies.Append("SelectedAccountNumber", accountNumber, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    HttpOnly = true
                });
                HttpContext.Session.SetString("SelectedAccountNumber", accountNumber);
                TempData["SuccessMessage"] = $"Switched active portal view to account '{accountNumber}'.";
            }
            return RedirectToAction("Index", "Account");
        }
    }
}
