# HippoBank

A full-stack online banking portal built with ASP.NET Core, Blazor Server, Entity Framework Core, and SQL Server. Members can open multiple account types, view transaction history, and transfer funds between their accounts.

## Live Demo

_Deployment to Azure coming soon._

## Tech Stack

- **Backend:** ASP.NET Core 10, C#
- **Frontend:** Blazor Server (interactive server components)
- **Database:** Microsoft SQL Server (Azure SQL Edge in dev via Docker)
- **ORM:** Entity Framework Core 10
- **Auth:** ASP.NET Core Identity
- **Source Control:** Git / GitHub

## Features

- Secure user registration and login via ASP.NET Core Identity
- Multiple account types per member (Checking, Savings) with type-specific behavior
- Open new accounts with a configurable opening deposit
- Transfer money between your own accounts
- Full transaction history per account
- Per-user data isolation (members can only access their own accounts)

## Design Decisions

### Object-Oriented Account Hierarchy

Accounts are modeled with an abstract base class and concrete subclasses, mapped to a single table using Entity Framework Core's Table-Per-Hierarchy (TPH) inheritance:

```
Account (abstract)
├── CheckingAccount
└── SavingsAccount
```

Each subclass overrides `AccountType` and `CalculateMonthlyInterest()`. The `TransferService` operates on the abstract `Account` type via polymorphism — it doesn't need to know which concrete subclass it's dealing with.

### Money Math

All monetary values use C#'s `decimal` type and SQL Server's `decimal(19,4)` column type. `double` and `float` are never used for money because their binary floating-point representation introduces rounding errors that compound across transactions.

### Database Transactions

Every transfer is wrapped in an explicit database transaction (`BeginTransactionAsync`). The debit from the source account and the credit to the destination account either both succeed or both roll back — there is no state where money disappears mid-transfer.

### Double-Entry Bookkeeping

Each transfer writes two `Transaction` rows: one `TransferOut` against the source account and one `TransferIn` against the destination. This mirrors how real financial systems track money movement and makes the ledger fully auditable.

### Authorization at the Data Layer

Every query that touches an account filters by the authenticated user's ID. There is no path to view or modify another member's account.

### Secrets Management

The SQL Server connection string is stored in .NET user secrets locally and would be stored in Azure Key Vault (or App Service configuration) in production. `appsettings.json` contains only a placeholder.

## Database Schema

Identity tables (`AspNetUsers`, etc.) are provided by ASP.NET Core Identity. Domain tables:

- `Accounts` — TPH table with discriminator column for Checking vs Savings
- `Transactions` — one row per money movement, with foreign key to `Accounts`

## Running Locally

### Prerequisites

- .NET 10 SDK
- Docker (for SQL Server)
- A SQL Server connection string

### Setup

1. Clone the repo:

```bash
   git clone https://github.com/ReeseBrockman/hippobank.git
   cd hippobank
```

2. Start SQL Server in Docker:

```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/azure-sql-edge
```

3. Set the connection string in user secrets:

```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=HippoBank;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
```

4. Apply database migrations:

```bash
   dotnet ef database update
```

5. Run the app:

```bash
   dotnet run
```

6. Open `http://localhost:5046` and register an account.

## Roadmap

- Member-to-member P2P transfers with idempotency keys
- Staff role with account administration
- Audit log for all sensitive operations
- Scheduled transfers and recurring payments
- Interest accrual job (monthly)
- Loan accounts with amortization schedules
- Deployment to Azure App Service with Azure SQL Database
- xUnit unit tests for the service layer
- GitHub Actions CI/CD pipeline

## What I Learned

Building HippoBank involved working through several engineering concerns that don't appear in tutorial projects: getting decimal precision right for money, wrapping multi-row updates in database transactions, modeling polymorphic entities in a relational database, and isolating user data through both authentication and query-level filters. The project deliberately uses the Microsoft enterprise stack (ASP.NET Core, EF Core, SQL Server, Identity) end-to-end.
