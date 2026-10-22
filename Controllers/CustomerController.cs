using Microsoft.AspNetCore.Mvc;

namespace NWSDB.Web.Controllers
{
    public class CustomerController : Controller
    {
        // GET: /Customer/Index -> Redirects to /Admin/Customers
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Customers", "Admin");
        }

        // GET: /Customer/Create -> Redirects to /Admin/Customers/Create
        [HttpGet]
        public IActionResult Create()
        {
            return RedirectToAction("CreateCustomer", "Admin");
        }

        // GET: /Customer/Edit/{accountNumber} -> Redirects to /Admin/Customers/Edit/{accountNumber}
        [HttpGet]
        public IActionResult Edit(string accountNumber)
        {
            return RedirectToAction("EditCustomer", "Admin", new { accountNumber });
        }
    }
}
