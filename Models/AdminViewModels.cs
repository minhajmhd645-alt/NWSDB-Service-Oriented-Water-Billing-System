using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NWSDB.Web.Models
{
    public class AdminDashboardViewModel
    {
        public AdminStatsModel Stats { get; set; } = new();
        public IEnumerable<CustomerModel> RecentCustomers { get; set; } = Array.Empty<CustomerModel>();
        public IEnumerable<BillModel> RecentBills { get; set; } = Array.Empty<BillModel>();
        public IEnumerable<PaymentModel> RecentPayments { get; set; } = Array.Empty<PaymentModel>();
    }

    public class CustomerListViewModel
    {
        public IEnumerable<CustomerModel> Customers { get; set; } = Array.Empty<CustomerModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public string FilterStatus { get; set; } = "All";
    }

    public class AdminCreateCustomerViewModel
    {
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Registered Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email Address cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Account Status")]
        public string AccountStatus { get; set; } = "Active";

        [Display(Name = "Initial Bill Amount (LKR)")]
        [Range(0, 1000000, ErrorMessage = "Initial bill amount must be non-negative.")]
        public decimal InitialBillAmount { get; set; } = 0;
    }

    public class AdminEditCustomerViewModel
    {
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Registered Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number format.")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
        [StringLength(100, ErrorMessage = "Email Address cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Account Status")]
        public string AccountStatus { get; set; } = "Active";
    }

    public class BillListViewModel
    {
        public IEnumerable<BillModel> Bills { get; set; } = Array.Empty<BillModel>();
        public string SearchAccount { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = "All"; // All, Unpaid, Paid, Overdue
    }

    public class CreateBillViewModel
    {
        [Required(ErrorMessage = "Customer Account Number is required.")]
        [Display(Name = "Customer Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Billing Month is required.")]
        [Display(Name = "Billing Month (e.g., Oct-2026)")]
        public string BillingMonth { get; set; } = DateTime.UtcNow.ToString("MMM-yyyy");

        [Required(ErrorMessage = "Previous Meter Reading is required.")]
        [Range(0, 999999, ErrorMessage = "Previous reading must be non-negative.")]
        [Display(Name = "Previous Meter Reading")]
        public int PreviousReading { get; set; }

        [Required(ErrorMessage = "Current Meter Reading is required.")]
        [Range(0, 999999, ErrorMessage = "Current reading must be non-negative.")]
        [Display(Name = "Current Meter Reading")]
        public int CurrentReading { get; set; }

        [Display(Name = "Bill Amount (LKR)")]
        [Range(0.00, 1000000.00, ErrorMessage = "Bill amount must be positive.")]
        public decimal BillAmount { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);

        [Display(Name = "Bill Status")]
        public string Status { get; set; } = "Unpaid";
    }

    public class EditBillViewModel
    {
        public int BillId { get; set; }

        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Billing Month is required.")]
        [Display(Name = "Billing Month")]
        public string BillingMonth { get; set; } = string.Empty;

        [Required(ErrorMessage = "Previous Meter Reading is required.")]
        [Range(0, 999999, ErrorMessage = "Previous reading must be non-negative.")]
        [Display(Name = "Previous Meter Reading")]
        public int PreviousReading { get; set; }

        [Required(ErrorMessage = "Current Meter Reading is required.")]
        [Range(0, 999999, ErrorMessage = "Current reading must be non-negative.")]
        [Display(Name = "Current Meter Reading")]
        public int CurrentReading { get; set; }

        [Required(ErrorMessage = "Bill Amount is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Bill amount must be greater than 0.")]
        [Display(Name = "Bill Amount (LKR)")]
        public decimal BillAmount { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Unpaid";
    }

    public class PaymentListViewModel
    {
        public IEnumerable<PaymentModel> Payments { get; set; } = Array.Empty<PaymentModel>();
        public string SearchAccount { get; set; } = string.Empty;
        public string MethodFilter { get; set; } = "All";
    }
}
