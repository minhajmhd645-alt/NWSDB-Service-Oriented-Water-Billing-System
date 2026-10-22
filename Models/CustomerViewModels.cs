using System.ComponentModel.DataAnnotations;

namespace NWSDB.Web.Models
{
    public class CreateCustomerViewModel
    {
        [Required(ErrorMessage = "Please enter an account number.")]
        [MaxLength(20, ErrorMessage = "Account number cannot exceed 20 characters.")]
        [Display(Name = "NWSDB Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter full customer name.")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter registered address.")]
        [MaxLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Registered Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter contact phone number.")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }

    public class EditCustomerViewModel
    {
        [Required]
        [Display(Name = "NWSDB Account Number")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter full customer name.")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter registered address.")]
        [MaxLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Registered Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter contact phone number.")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
    }
}
