# Part 5: Third-Party Client Architecture Documentation

## 1. Overview & Purpose
The **NWSDB Third-Party Client** (`NWSDB.ThirdPartyClient`) is a standalone ASP.NET Core MVC application representing an external partner organization ("WaterPay Partner Portal").

It demonstrates how third-party organizations can seamlessly integrate with the National Water Supply and Drainage Board (NWSDB) services via RESTful APIs without accessing internal databases or core banking APIs directly.

---

## 2. Architecture & Data Flow Boundary

```
       ┌────────────────────────────┐
       │    THIRD-PARTY CLIENT      │
       │    WaterPay Portal         │
       │   (ASP.NET Core MVC)       │
       └─────────────┬──────────────┘
                     │
                     ▼ REST API (HTTPS)
               ┌─────────────┐
               │  NWSDB API  │
               └──────┬──────┘
                      │
           ┌──────────┴──────────┐
           ▼                     ▼
     ┌───────────┐         ┌───────────┐
     │ NWSDB DB  │         │ Bank API  │
     └───────────┘         └─────┬─────┘
                                 ▼
                           ┌───────────┐
                           │  Bank DB  │
                           └───────────┘
```

### Strict Database Boundary Enforced
- **Zero direct SQL connections**: The Third-Party Client does NOT contain connection strings or SQL queries.
- **Zero EF Core dependencies**: No Entity Framework Core or database packages are included in `NWSDB.ThirdPartyClient.csproj`.
- **Zero direct Bank API calls**: Payment requests are submitted exclusively to `POST /api/v1/payments` on `NWSDB.API`. The NWSDB API coordinates downstream bank processing.

---

## 3. Technologies Used
- **Language & Framework**: C# / .NET 8 (`net10.0` SDK compatibility)
- **Architecture**: ASP.NET Core MVC with Razor Views
- **API Client**: `INwsdbApiClient` / `NwsdbApiClient` using `IHttpClientFactory` & typed `HttpClient`
- **UI & Styling**: Bootstrap 5, Bootstrap Icons, Google Fonts (Plus Jakarta Sans), Custom CSS

---

## 4. API Endpoints Consumed

| Domain | HTTP Method | Endpoint Route | Client Method |
| :--- | :--- | :--- | :--- |
| **Customer Lookup** | `GET` | `/api/v1/customers/{accountNumber}` | `GetCustomerAsync` |
| **Customer Summary** | `GET` | `/api/v1/customers/{accountNumber}/summary` | `GetCustomerSummaryAsync` |
| **Bill History** | `GET` | `/api/v1/bills/{accountNumber}` | `GetBillsAsync` |
| **Unpaid Bills** | `GET` | `/api/v1/bills/{accountNumber}/unpaid` | `GetUnpaidBillsAsync` |
| **Water Usage** | `GET` | `/api/v1/usage/{accountNumber}/current` | `GetCurrentUsageAsync` |
| **Payment History** | `GET` | `/api/v1/payments/{accountNumber}` | `GetPaymentsAsync` |
| **Process Payment** | `POST` | `/api/v1/payments` | `ProcessPaymentAsync` |

---

## 5. Client Application Architecture

```
NWSDB.ThirdPartyClient/
├── Controllers/
│   ├── HomeController.cs        # Landing page & dashboard
│   ├── CustomerController.cs    # Lookup & Customer Summary
│   ├── BillsController.cs       # Billing history & Unpaid bills
│   ├── UsageController.cs       # Meter readings & unit usage
│   └── PaymentController.cs     # Payment form, processing & history
├── Services/
│   ├── INwsdbApiClient.cs       # Client abstraction interface
│   └── NwsdbApiClient.cs        # Typed HttpClient implementation
├── Models/
│   ├── ApiModels.cs             # API DTO contracts
│   └── ViewModels.cs            # MVC View models
├── Views/
│   ├── Customer/                # Index.cshtml, Details.cshtml
│   ├── Bills/                   # Index.cshtml, Unpaid.cshtml
│   ├── Usage/                   # Index.cshtml
│   ├── Payment/                 # Pay.cshtml, Success.cshtml, Failure.cshtml, History.cshtml
│   └── Shared/                  # _Layout.cshtml, _ValidationScriptsPartial.cshtml, Error.cshtml
├── appsettings.json             # Configurable ApiSettings:BaseUrl
└── Program.cs                   # DI registration & typed client setup
```

---

## 6. Payment Flow

1. **User Request**: User selects an unpaid bill and chooses a bank account on Third-Party Client.
2. **Client Post**: Third-Party Client posts request to `POST /api/v1/payments` on `NWSDB.API`.
3. **NWSDB API Validation**: NWSDB API validates bill existence and status.
4. **Bank Processing**: NWSDB API calls `POST /api/v1/bank/payments` on `Bank.API`.
5. **Settlement**: On bank approval, NWSDB API marks the bill as `Paid` and records a payment record.
6. **Result View**: Third-Party Client receives the result and displays `Success.cshtml` or `Failure.cshtml`.

---

## 7. Configuration & Ports
- **NWSDB API URL**: `https://localhost:7194`
- **Third-Party Client Development Ports**:
  - HTTPS: `https://localhost:7060`
  - HTTP: `http://localhost:5060`
