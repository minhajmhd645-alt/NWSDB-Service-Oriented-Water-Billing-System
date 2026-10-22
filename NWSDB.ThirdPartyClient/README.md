# WaterPay Partner Portal (NWSDB Third-Party Client)

WaterPay Partner Portal is a standalone ASP.NET Core MVC application developed as Part 5 of the NWSDB Service-Oriented Computing Assignment.

## Features
- **Customer Lookup**: Retrieve customer demographic details.
- **Customer Summary Dashboard**: Overview of outstanding balance, unpaid bills, current water usage, and latest payment.
- **Bill History**: Complete billing statements for customer accounts.
- **Unpaid Bills Filter**: Filter pending bills with direct payment options.
- **Water Usage**: View meter readings and monthly consumption.
- **Payment History**: Audit trail of processed payments.
- **Integrated Payment Checkout**: Initiate payments through NWSDB API.

## Running the Application

### 1. Ensure Prerequisites
Make sure `Bank.API` and `NWSDB.API` are running:
- **Bank.API**: `https://localhost:7001`
- **NWSDB.API**: `https://localhost:7194`

### 2. Start Third-Party Client
Run from project directory:
```bash
dotnet run --launch-profile https
```

The application will be accessible at:
- **HTTPS**: `https://localhost:7060`
- **HTTP**: `http://localhost:5060`

### 3. Test Accounts
- **NWSDB10004** (Samanthi de Silva - Negombo)
- **NWSDB10001** (Sunil Perera - Colombo 03)
- **NWSDB10005** (Anura Bandara - Kurunegala)
