using System;
using System.Linq;
using NWSDB.API.Models;

namespace NWSDB.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(NWSDBDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            var customers = new Customer[]
            {
                new Customer { AccountNumber = "NWSDB10001", FullName = "Sunil Perera", Address = "Colombo 03", PhoneNumber = "0711111111", Email = "sunil@example.com" },
                new Customer { AccountNumber = "NWSDB10002", FullName = "Nimal Silva", Address = "Kandy", PhoneNumber = "0722222222", Email = "nimal@example.com" },
                new Customer { AccountNumber = "NWSDB10003", FullName = "Kumara Fernando", Address = "Galle", PhoneNumber = "0733333333", Email = "kumara@example.com" },
                new Customer { AccountNumber = "NWSDB10004", FullName = "Samanthi de Silva", Address = "Negombo", PhoneNumber = "0744444444", Email = "samanthi@example.com" },
                new Customer { AccountNumber = "NWSDB10005", FullName = "Anura Bandara", Address = "Kurunegala", PhoneNumber = "0755555555", Email = "anura@example.com" },
                new Customer { AccountNumber = "NWSDB10006", FullName = "Chamara Weerasinghe", Address = "Matara", PhoneNumber = "0766666666", Email = "chamara@example.com" },
                new Customer { AccountNumber = "NWSDB10007", FullName = "Nuwan Pradeep", Address = "Gampaha", PhoneNumber = "0777777777", Email = "nuwan@example.com" },
                new Customer { AccountNumber = "NWSDB10008", FullName = "Kasun Kalhara", Address = "Kalutara", PhoneNumber = "0788888888", Email = "kasun@example.com" },
                new Customer { AccountNumber = "NWSDB10009", FullName = "Damith Asanka", Address = "Ratnapura", PhoneNumber = "0799999999", Email = "damith@example.com" },
                new Customer { AccountNumber = "NWSDB10010", FullName = "Lahiru Thirimanne", Address = "Anuradhapura", PhoneNumber = "0700000000", Email = "lahiru@example.com" }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

            var currentMonth = DateTime.UtcNow.ToString("MMM-yyyy");
            var lastMonth = DateTime.UtcNow.AddMonths(-1).ToString("MMM-yyyy");

            var bills = new Bill[]
            {
                new Bill { AccountNumber = "NWSDB10001", BillingMonth = lastMonth, PreviousReading = 100, CurrentReading = 150, UnitsConsumed = 50, BillAmount = 1500, DueDate = DateTime.UtcNow.AddDays(-5), Status = "Paid" },
                new Bill { AccountNumber = "NWSDB10001", BillingMonth = currentMonth, PreviousReading = 150, CurrentReading = 210, UnitsConsumed = 60, BillAmount = 1800, DueDate = DateTime.UtcNow.AddDays(10), Status = "Unpaid" },
                
                new Bill { AccountNumber = "NWSDB10002", BillingMonth = lastMonth, PreviousReading = 200, CurrentReading = 230, UnitsConsumed = 30, BillAmount = 900, DueDate = DateTime.UtcNow.AddDays(-10), Status = "Overdue" },
                new Bill { AccountNumber = "NWSDB10002", BillingMonth = currentMonth, PreviousReading = 230, CurrentReading = 280, UnitsConsumed = 50, BillAmount = 1500, DueDate = DateTime.UtcNow.AddDays(5), Status = "Unpaid" },

                new Bill { AccountNumber = "NWSDB10003", BillingMonth = lastMonth, PreviousReading = 50, CurrentReading = 120, UnitsConsumed = 70, BillAmount = 2100, DueDate = DateTime.UtcNow.AddDays(-1), Status = "Paid" },
                new Bill { AccountNumber = "NWSDB10003", BillingMonth = currentMonth, PreviousReading = 120, CurrentReading = 190, UnitsConsumed = 70, BillAmount = 2100, DueDate = DateTime.UtcNow.AddDays(15), Status = "Unpaid" },

                new Bill { AccountNumber = "NWSDB10004", BillingMonth = lastMonth, PreviousReading = 10, CurrentReading = 40, UnitsConsumed = 30, BillAmount = 900, DueDate = DateTime.UtcNow.AddDays(-20), Status = "Overdue" },
                new Bill { AccountNumber = "NWSDB10004", BillingMonth = currentMonth, PreviousReading = 40, CurrentReading = 90, UnitsConsumed = 50, BillAmount = 1500, DueDate = DateTime.UtcNow.AddDays(2), Status = "Unpaid" },

                new Bill { AccountNumber = "NWSDB10005", BillingMonth = currentMonth, PreviousReading = 300, CurrentReading = 340, UnitsConsumed = 40, BillAmount = 1200, DueDate = DateTime.UtcNow.AddDays(8), Status = "Unpaid" },
                
                // Add more bills for the rest
                new Bill { AccountNumber = "NWSDB10006", BillingMonth = currentMonth, PreviousReading = 0, CurrentReading = 55, UnitsConsumed = 55, BillAmount = 1650, DueDate = DateTime.UtcNow.AddDays(12), Status = "Unpaid" },
                new Bill { AccountNumber = "NWSDB10007", BillingMonth = currentMonth, PreviousReading = 110, CurrentReading = 145, UnitsConsumed = 35, BillAmount = 1050, DueDate = DateTime.UtcNow.AddDays(10), Status = "Unpaid" },
                new Bill { AccountNumber = "NWSDB10008", BillingMonth = currentMonth, PreviousReading = 210, CurrentReading = 275, UnitsConsumed = 65, BillAmount = 1950, DueDate = DateTime.UtcNow.AddDays(14), Status = "Unpaid" },
                new Bill { AccountNumber = "NWSDB10009", BillingMonth = currentMonth, PreviousReading = 320, CurrentReading = 380, UnitsConsumed = 60, BillAmount = 1800, DueDate = DateTime.UtcNow.AddDays(11), Status = "Unpaid" },
                new Bill { AccountNumber = "NWSDB10010", BillingMonth = currentMonth, PreviousReading = 410, CurrentReading = 450, UnitsConsumed = 40, BillAmount = 1200, DueDate = DateTime.UtcNow.AddDays(9), Status = "Unpaid" }
            };
            context.Bills.AddRange(bills);
            context.SaveChanges();

            var usages = new WaterUsage[]
            {
                new WaterUsage { AccountNumber = "NWSDB10001", ReadingMonth = lastMonth, PreviousReading = 100, CurrentReading = 150, UnitsConsumed = 50, RecordedAt = DateTime.UtcNow.AddDays(-20) },
                new WaterUsage { AccountNumber = "NWSDB10001", ReadingMonth = currentMonth, PreviousReading = 150, CurrentReading = 210, UnitsConsumed = 60, RecordedAt = DateTime.UtcNow.AddDays(-2) },
                new WaterUsage { AccountNumber = "NWSDB10002", ReadingMonth = lastMonth, PreviousReading = 200, CurrentReading = 230, UnitsConsumed = 30, RecordedAt = DateTime.UtcNow.AddDays(-22) },
                new WaterUsage { AccountNumber = "NWSDB10002", ReadingMonth = currentMonth, PreviousReading = 230, CurrentReading = 280, UnitsConsumed = 50, RecordedAt = DateTime.UtcNow.AddDays(-3) }
            };
            context.WaterUsages.AddRange(usages);
            context.SaveChanges();

            // Only add payments for bills that are marked as Paid
            var paidBills = context.Bills.Where(b => b.Status == "Paid").ToList();
            foreach (var bill in paidBills)
            {
                var payment = new Payment
                {
                    BillId = bill.BillId,
                    AccountNumber = bill.AccountNumber,
                    Amount = bill.BillAmount,
                    TransactionId = "TXN" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    PaymentMethod = "DemoCard",
                    Status = "Success",
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                };
                context.Payments.Add(payment);
            }
            context.SaveChanges();
        }
    }
}
