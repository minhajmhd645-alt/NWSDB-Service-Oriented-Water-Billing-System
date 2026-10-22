# NWSDB API

## 1. Project Purpose

This is **Part 1** of a Service-Oriented Computing (SOC) university assignment based on the Sri Lanka National Water Supply and Drainage Board (NWSDB).

The NWSDB API exposes water utility services through a RESTful Web API. It handles customer information, water usage records, billing, and a demo payment mechanism. The API is designed to be consumed by:
- The NWSDB Website (future Part 2)
- Third-party client applications (future Part 3)

---

## 2. Architecture

```
Client (Web/Third-Party)
         ↓
   NWSDB REST API
         ↓
     Controllers
         ↓
  Service Interfaces
         ↓
     Services (Business Logic)
         ↓
  EF Core (Repository Pattern)
         ↓
  SQL Server (NWSDBDb)
```

### Project Structure

```
NWSDB.API/
├── Controllers/          # Thin controllers — delegate to services
├── Services/             # Business logic implementations
├── Interfaces/           # Service contracts
├── DTOs/                 # Data Transfer Objects (Customers, Bills, WaterUsage, Payments)
├── Models/               # EF Core entity models
├── Data/                 # DbContext and seed data
├── Middleware/           # Global exception handling
├── Migrations/           # EF Core migrations
├── appsettings.json
└── Program.cs

NWSDB.API.Tests/
├── CustomerServiceTests.cs
├── BillServiceTests.cs
└── PaymentServiceTests.cs
```

---

## 3. Technologies

| Technology | Purpose |
|---|---|
| C# / .NET 8 | Runtime and language |
| ASP.NET Core Web API | REST API framework |
| Entity Framework Core | ORM / Data Access |
| SQL Server (LocalDB) | Database |
| Scalar API Reference | Interactive API documentation |
| xUnit | Unit testing |
| Microsoft.EntityFrameworkCore.InMemory | In-memory DB for testing |

---

## 4. Database Setup

Database name: `NWSDBDb`

Requires SQL Server or SQL Server LocalDB installed.

---

## 5. Connection String

Edit `NWSDB.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NWSDBDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

For a full SQL Server instance:
```json
"DefaultConnection": "Server=localhost;Database=NWSDBDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

---

## 6. EF Core Migrations

```bash
# Install EF Core global tool (one-time)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply to database (creates NWSDBDb and all tables)
dotnet ef database update
```

---

## 7. How to Run

```bash
# From the solution root
dotnet restore
dotnet build

# Apply migrations (if not already done)
cd NWSDB.API
dotnet ef database update

# Run the API
dotnet run
```

The API starts at: `http://localhost:5243`

---

## 8. Scalar API Reference

Scalar is available in **Development** mode at:

```
http://localhost:5243/scalar/v1
```

OpenAPI document:
```
http://localhost:5243/openapi/v1.json
```

> **Note:** Swagger UI is NOT used. Scalar serves as the interactive API reference.

---

## 9. API Endpoints

### Health

| Method | Route | Description |
|---|---|---|
| GET | `/health` | Health status |
| GET | `/health/db` | Database health check |

### Customers

| Method | Route | Description |
|---|---|---|
| GET | `/api/v1/customers/{accountNumber}` | Get customer details |
| GET | `/api/v1/customers/{accountNumber}/summary` | Get customer summary |

### Bills

| Method | Route | Description |
|---|---|---|
| GET | `/api/v1/bills/{accountNumber}` | Get all bills for an account |
| GET | `/api/v1/bills/{accountNumber}/unpaid` | Get unpaid/overdue bills |
| GET | `/api/v1/bills/id/{billId}` | Get a specific bill by ID |

### Water Usage

| Method | Route | Description |
|---|---|---|
| GET | `/api/v1/usage/{accountNumber}` | Get usage history |
| GET | `/api/v1/usage/{accountNumber}/current` | Get current/latest usage |

### Payments

| Method | Route | Description |
|---|---|---|
| GET | `/api/v1/payments/{accountNumber}` | Get payment history |
| GET | `/api/v1/payments/id/{paymentId}` | Get a specific payment |
| POST | `/api/v1/payments` | Create a demo payment |

---

## 10. Sample Requests

### Get Customer
```http
GET http://localhost:5243/api/v1/customers/NWSDB10001
```

### Get Customer Summary
```http
GET http://localhost:5243/api/v1/customers/NWSDB10001/summary
```

### Get Bills
```http
GET http://localhost:5243/api/v1/bills/NWSDB10001
GET http://localhost:5243/api/v1/bills/NWSDB10001/unpaid
GET http://localhost:5243/api/v1/bills/id/1
```

### Get Usage
```http
GET http://localhost:5243/api/v1/usage/NWSDB10001
GET http://localhost:5243/api/v1/usage/NWSDB10001/current
```

### Create Payment (POST)
```http
POST http://localhost:5243/api/v1/payments
Content-Type: application/json

{
  "billId": 2,
  "accountNumber": "NWSDB10001",
  "amount": 1800.00,
  "paymentMethod": "DemoCard"
}
```

Valid `paymentMethod` values: `DemoCard`, `DemoBankAccount`

---

## 11. Sample Responses

### Success
```json
{
  "success": true,
  "message": "Customer retrieved successfully.",
  "data": {
    "accountNumber": "NWSDB10001",
    "fullName": "Sunil Perera",
    "address": "Colombo 03",
    "phoneNumber": "0711111111",
    "email": "sunil@example.com"
  },
  "timestamp": "2026-09-17T07:01:52.1102443Z"
}
```

### Error (Not Found)
```json
{
  "success": false,
  "message": "Customer not found.",
  "data": null,
  "timestamp": "2026-09-17T07:01:52.1102443Z"
}
```

### Payment Created (201)
```json
{
  "success": true,
  "message": "Payment created successfully.",
  "data": {
    "paymentId": 3,
    "billId": 2,
    "amount": 1800.00,
    "transactionId": "TXNBE39C610",
    "paymentMethod": "DemoCard",
    "status": "Success",
    "paymentDate": "2026-09-17T07:05:05.6537533Z"
  },
  "timestamp": "2026-09-17T07:05:05.8041352Z"
}
```

---

## 12. Testing

### Run Unit Tests
```bash
cd NWSDB.API.Tests
dotnet test --verbosity normal
```

### Test Coverage

| Test Class | Tests |
|---|---|
| `CustomerServiceTests` | Existing customer, Non-existing customer, Summary (exists), Summary (not found) |
| `BillServiceTests` | All bills, No bills, Unpaid only, Specific bill, Non-existing bill |
| `PaymentServiceTests` | Valid payment, Non-existing customer, Non-existing bill, Bill belongs to another customer, Already paid bill, Zero amount, Invalid payment method |

**Total: 17 tests — all passing**

---

## 13. Future Bank API Integration (Part 3)

In Part 3, `PaymentService.CreatePaymentAsync` will be extended to:

1. Call the external `Bank.API` with the customer's demo payment credentials
2. Receive a real transaction result (Success/Failed)
3. Update the Bill and Payment status accordingly

The current implementation simulates this by auto-approving demo payments with a generated `TransactionId` — no real bank calls are made.

---

## Seed Data

The following fictional accounts are pre-seeded:

| Account | Name |
|---|---|
| NWSDB10001 | Sunil Perera |
| NWSDB10002 | Nimal Silva |
| NWSDB10003 | Kumara Fernando |
| NWSDB10004 | Samanthi de Silva |
| NWSDB10005 | Anura Bandara |
| NWSDB10006 | Chamara Weerasinghe |
| NWSDB10007 | Nuwan Pradeep |
| NWSDB10008 | Kasun Kalhara |
| NWSDB10009 | Damith Asanka |
| NWSDB10010 | Lahiru Thirimanne |
