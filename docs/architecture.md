# NWSDB API Architecture

## System Overview

This document describes the architecture of the NWSDB API — Part 1 of the Service-Oriented Computing (SOC) assignment.

---

## Architecture Diagram

```
┌──────────────────────────────────────────────────────┐
│                  NWSDB Clients                        │
│                                                       │
│   NWSDB Web App          Third-Party Applications     │
│   (Future - Part 2)      (Future - Part 3)            │
└──────────────────────┬───────────────────────────────┘
                       │  HTTP/HTTPS REST Requests
                       ▼
┌──────────────────────────────────────────────────────┐
│                   NWSDB REST API                      │
│              ASP.NET Core Web API (.NET 8)            │
│                  http://localhost:5243                │
│                                                       │
│  ┌──────────────────────────────────────────────┐    │
│  │              Global Exception Middleware      │    │
│  │         ExceptionHandlingMiddleware.cs        │    │
│  └──────────────────────────────────────────────┘    │
│                                                       │
│  ┌────────────────────────────────────────────────┐  │
│  │                  Controllers                   │  │
│  │                                                │  │
│  │  CustomersController  /api/v1/customers/...   │  │
│  │  BillsController      /api/v1/bills/...       │  │
│  │  UsageController      /api/v1/usage/...       │  │
│  │  PaymentsController   /api/v1/payments/...    │  │
│  │  HealthController     /health                  │  │
│  └────────────────────────────────────────────────┘  │
│                       │                              │
│  ┌────────────────────▼───────────────────────────┐  │
│  │              Service Interfaces                │  │
│  │                                                │  │
│  │  ICustomerService    IBillService              │  │
│  │  IWaterUsageService  IPaymentService           │  │
│  └────────────────────┬───────────────────────────┘  │
│                       │  Dependency Injection         │
│  ┌────────────────────▼───────────────────────────┐  │
│  │               Service Layer                    │  │
│  │           (Business Logic Layer)               │  │
│  │                                                │  │
│  │  CustomerService     BillService               │  │
│  │  WaterUsageService   PaymentService            │  │
│  └────────────────────┬───────────────────────────┘  │
│                       │                              │
│  ┌────────────────────▼───────────────────────────┐  │
│  │           EF Core / Repository Layer           │  │
│  │                                                │  │
│  │  NWSDBDbContext                                │  │
│  │  (Customers, Bills, WaterUsages, Payments)     │  │
│  └────────────────────┬───────────────────────────┘  │
└──────────────────────┬───────────────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────────────┐
│                   SQL Server                          │
│               Database: NWSDBDb                      │
│                                                       │
│   Tables:  Customers                                  │
│            Bills                                      │
│            WaterUsages                               │
│            Payments                                   │
└──────────────────────────────────────────────────────┘
```

---

## Component Responsibilities

### Controllers
- Receive HTTP requests
- Validate ModelState
- Delegate all business logic to services
- Return `ResponseDto<T>` wrapper with appropriate HTTP status codes
- No business logic should exist in controllers

### Service Interfaces (Interfaces/)
- Define the contract for each service
- Enable unit testing via dependency injection
- Enable future service substitution (e.g., mocked services)

### Services (Services/)
- Contain all business logic
- Query EF Core directly (no separate repository layer needed at this scale)
- Map domain entities to DTOs
- Validate business rules (e.g., bill ownership, payment status)

### EF Core / NWSDBDbContext
- Manages database access via LINQ
- Handles entity relationships (Customer → Bills, Customer → WaterUsages, Bill → Payments)
- Supports async operations throughout

### DTOs (DTOs/)
- `ResponseDto<T>` — uniform JSON response wrapper for all endpoints
- Customer, Bill, WaterUsage, Payment request/response DTOs
- Prevent direct entity exposure to clients

### Middleware (Middleware/)
- `ExceptionHandlingMiddleware` — catches all unhandled exceptions globally
- Returns clean JSON error responses
- Logs technical details without exposing them to clients

---

## Database Schema

```
Customers
  ├── CustomerId (PK)
  ├── AccountNumber (UNIQUE)
  ├── FullName, Address, PhoneNumber, Email
  └── CreatedAt

Bills
  ├── BillId (PK)
  ├── AccountNumber (FK → Customers.AccountNumber)
  ├── BillingMonth, PreviousReading, CurrentReading
  ├── UnitsConsumed, BillAmount, DueDate
  ├── Status (Unpaid | Paid | Overdue)
  └── CreatedAt

WaterUsages
  ├── UsageId (PK)
  ├── AccountNumber (FK → Customers.AccountNumber)
  ├── ReadingMonth, PreviousReading, CurrentReading
  ├── UnitsConsumed
  └── RecordedAt

Payments
  ├── PaymentId (PK)
  ├── BillId (FK → Bills.BillId)
  ├── AccountNumber
  ├── Amount, TransactionId, PaymentMethod
  ├── Status (Pending | Success | Failed)
  ├── PaymentDate
  └── CreatedAt
```

---

## Planned Future Integration: Bank API (Part 3)

In Part 3, the `PaymentService.CreatePaymentAsync` will be extended to:

1. Call `Bank.API` with demo payment credentials
2. Receive authorization response (Success/Failed)
3. Update the local payment and bill records accordingly

```
NWSDB API (PaymentService)
       ↓  HTTP POST
   Bank.API
       ↓  Response
NWSDB API (updates Payment + Bill status)
```

The current demo implementation auto-approves payments and generates a random `TransactionId` to simulate successful bank responses without real bank calls.

---

## Security Considerations

- Input validation via DTOs and service-layer checks
- No real card numbers, credentials, or PII stored
- Database credentials never hard-coded (configuration-driven)
- Internal exceptions never exposed to API clients
- CORS configured for future front-end clients
- No authentication in Part 1 (to be introduced in later stages)
