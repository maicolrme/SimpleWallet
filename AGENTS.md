# AGENTS.md

## Project

This repository contains a simple **ASP.NET Core MVC Wallet MVP**.

The application is intentionally small and should remain easy to understand, modify and deploy.

The goal is to build a realistic corporate wallet interface without unnecessary architecture or complexity.

---

# Version

Current project version:

```text
v2
```

When making significant architectural or functional changes, update the project version.

Example:

```text
v1 → v1.1
v1.1 → v2
```

Do not change the version for minor bug fixes unless requested.

---

# Main Principles

Always prioritize:

1. Simplicity
2. Readability
3. Maintainability
4. Security
5. Consistent UI
6. Fast development

Avoid introducing architecture only because it is theoretically cleaner.

This is an MVP.

---

# Technology

Use:

* ASP.NET Core MVC
* C#
* Entity Framework Core
* SQLite
* Razor Views
* Tailwind CSS via CDN
* Docker
* Docker Compose

Do not introduce another frontend framework.

Do not introduce unnecessary infrastructure.

---

# Architecture

Use a simple MVC + Services architecture.

```text
Request
   ↓
Controller
   ↓
Service
   ↓
Entity Framework Core
   ↓
SQLite
```

Controllers should be thin.

Services contain business logic.

Models represent application data.

DbContext handles persistence.

---

# Controllers

Controllers are responsible for:

* Receiving HTTP requests
* Validating request models
* Calling services
* Returning Views
* Redirecting users
* Returning appropriate HTTP responses

Controllers should NOT contain complex business logic.

### Good

```csharp
public async Task<IActionResult> Send(SendMoneyViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    await _transactionService.SendAsync(
        GetCurrentUserId(),
        model.RecipientEmail,
        model.Amount,
        model.Description
    );

    return RedirectToAction(nameof(Index));
}
```

### Avoid

```csharp
public async Task<IActionResult> Send(SendMoneyViewModel model)
{
    // Find wallet
    // Find recipient
    // Validate balance
    // Modify balances
    // Create transaction
    // Save database
    // ...
}
```

Move that logic into a service.

---

# Services

Business logic belongs inside `Services/`.

Recommended services:

```text
Services/
├── AuthService.cs
├── UserService.cs
├── WalletService.cs
├── TransactionService.cs
└── AdminService.cs
```

Services should contain the logic required to perform business operations.

---

## AuthService

Responsible for:

* User registration
* Password hashing
* Password verification
* Authentication-related logic
* Creating users

Do not store passwords as plain text.

---

## UserService

Responsible for:

* Getting users
* Updating profiles
* Changing user information
* Activating/deactivating users
* Basic user management

---

## WalletService

Responsible for:

* Creating wallets
* Getting wallet information
* Reading balances
* Deposits
* Withdrawals
* Balance updates

Example:

```csharp
public interface IWalletService
{
    Task<Wallet?> GetByUserIdAsync(int userId);

    Task DepositAsync(
        int userId,
        decimal amount,
        string description
    );

    Task<bool> WithdrawAsync(
        int userId,
        decimal amount,
        string description
    );
}
```

---

## TransactionService

Responsible for:

* Transfers
* Creating transactions
* Validating balances
* Transaction history
* Transaction status
* Transaction filtering

Example:

```csharp
public interface ITransactionService
{
    Task<Transaction> SendAsync(
        int senderUserId,
        string recipientEmail,
        decimal amount,
        string? description
    );

    Task<IReadOnlyList<Transaction>> GetUserTransactionsAsync(
        int userId
    );
}
```

---

## AdminService

Responsible for:

* Listing users
* User management
* Role management
* Wallet overview
* Transaction overview

Admin authorization must always be checked server-side.

---

# Dependency Injection

Use ASP.NET Core built-in dependency injection.

Register services in `Program.cs`.

Example:

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
```

Do not introduce a dependency injection framework.

---

# Interfaces

Use interfaces for services when they provide a clear benefit.

Do not create interfaces for every class automatically.

Good:

```text
IWalletService
WalletService
```

Avoid unnecessary abstractions such as:

```text
IWalletRepository
WalletRepository
IWalletManager
WalletManager
IWalletProcessor
WalletProcessor
```

unless the application genuinely requires them.

---

# Database

Use:

```text
SQLite
```

Entity Framework Core is responsible for persistence.

Database:

```text
wallet.db
```

Keep the domain model simple.

Main entities:

```text
User
Wallet
Transaction
```

Avoid unnecessary entities.

---

# Financial Logic

Wallet operations must never directly modify balances from Controllers.

All balance changes must go through `WalletService` or `TransactionService`.

Example:

```text
Controller
    ↓
TransactionService
    ↓
WalletService
    ↓
DbContext
```

Validate:

* Amount > 0
* Sender exists
* Recipient exists
* Sender is active
* Recipient is active
* Sender has sufficient balance
* Sender != recipient

Use `decimal` for monetary values.

Never use `double` or `float` for money.

---

# Transactions

Transfers should update the sender and recipient consistently.

For example:

```text
Sender:
€1000 → €900

Recipient:
€500 → €600

Transaction:
€100
Completed
```

Use a database transaction when multiple records must be updated atomically.

Do not allow partial balance updates.

---

# Authentication

Use ASP.NET Core Cookie Authentication.

Protected pages must require authentication.

Example:

```csharp
[Authorize]
public class DashboardController : Controller
{
}
```

Admin controllers must require the Admin role.

```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
}
```

Never rely only on UI visibility for authorization.

---

# Passwords

Never store:

```text
Password
PasswordPlainText
```

Use a secure password hashing mechanism.

Passwords must never appear in:

* Logs
* Database
* Views
* Exceptions
* API responses

---

# Validation

Validate all user input.

Examples:

```text
Email
Password
Amount
Description
Profile fields
```

For financial operations:

```text
Amount > 0
```

Do not trust values received from forms.

---

# Razor Views

Use Razor Views with Tailwind CSS.

Views should focus primarily on presentation.

Avoid putting business logic inside Razor.

Do not perform database queries directly from Views.

---

# UI Design

The application should look like a real small fintech product.

Style:

* Light theme
* White background
* WhatsApp-inspired green
* Clean typography
* Minimal borders
* Subtle shadows
* Moderate rounded corners
* Good spacing
* Responsive design

Primary color:

```text
#25D366
```

Dark green:

```text
#128C7E
```

Background:

```text
#FFFFFF
```

Surface:

```text
#F8FAF9
```

Text:

```text
#111827
```

Muted:

```text
#6B7280
```

Avoid excessive green.

Use green primarily for:

* Primary buttons
* Positive balances
* Success states
* Active navigation
* Important actions

---

# UI Philosophy

The application should NOT look like an AI-generated dashboard.

Avoid:

* Excessive gradients
* Glassmorphism
* Huge cards
* Excessive animations
* Decorative illustrations
* Random charts
* Excessive icons
* Unnecessary badges
* Overly futuristic UI

Prefer:

* Simple tables
* Clear forms
* Consistent spacing
* Strong typography
* Simple navigation
* Clear actions

The UI should resemble a professional internal fintech application.

---

# Tailwind

Tailwind is loaded through CDN.

Do not create a frontend build pipeline unless explicitly requested.

Example:

```html
<script src="https://cdn.tailwindcss.com"></script>
```

Prefer Tailwind utility classes directly in Razor.

Only create custom CSS when Tailwind cannot reasonably handle the requirement.

---

# Pages

Public:

```text
/
 /login
 /register
```

Authenticated:

```text
/dashboard
/wallet
/transactions
/profile
```

Admin:

```text
/admin
/admin/users
/admin/users/{id}
/admin/wallets
/admin/transactions
```

---

# Dashboard

The dashboard should show:

* Current balance
* Send action
* Receive action
* Recent transactions
* Total received
* Total sent
* Number of transactions

Keep the dashboard simple.

Do not introduce complex analytics.

---

# Wallet

Display:

* Balance
* Currency
* Wallet identifier/address
* Creation date

Actions:

```text
Send
Receive
```

For the MVP, wallet addresses are identifiers only.

No blockchain integration is required.

---

# Transactions

Display:

* Date
* Type
* Description
* Amount
* Status

Support simple filtering:

```text
All
Received
Sent
```

Avoid advanced filtering systems.

---

# Profile

Allow users to:

* View profile
* Edit name
* Edit surname
* Change password
* Logout

---

# Admin

Admin can:

* View users
* View user details
* Activate/deactivate users
* View wallets
* View transactions
* Manage basic roles

Keep administration simple.

---

# Seed Data

Development seed should create:

```text
Admin

Email:
admin@example.com

Password:
Admin123!
```

And:

```text
User

Email:
user@example.com

Password:
User123!
```

These credentials are development-only.

Never use these credentials in production.

---

# Docker

The application must run with:

```bash
docker compose up -d
```

Stop with:

```bash
docker compose down
```

The SQLite database must survive container recreation through a Docker volume.

Example:

```yaml
volumes:
  wallet_data:
```

Application:

```text
http://localhost:8080
```

---

# Dockerfile

Keep the Dockerfile simple.

Use a multi-stage .NET build if appropriate.

Do not add unnecessary containers.

For this MVP, one application container is enough.

SQLite runs inside the application container through the mounted persistent volume.

---

# Error Handling

Use simple application-level error handling.

Do not expose:

* Stack traces
* SQL errors
* Internal paths
* Sensitive information

Production should use an error page.

Development can display detailed errors.

---

# Logging

Use the standard ASP.NET Core logging system.

Log useful application events.

Do not log:

* Passwords
* Authentication cookies
* Sensitive financial information
* Secrets

---

# Configuration

Use:

```text
appsettings.json
appsettings.Development.json
```

Secrets should not be hardcoded.

For local development, environment variables can be used.

---

# Code Style

Use normal modern C# conventions.

Prefer:

```csharp
PascalCase
```

for:

* Classes
* Methods
* Properties

Use:

```csharp
camelCase
```

for:

* Local variables
* Parameters

Prefer async APIs:

```csharp
await service.GetAsync();
```

Avoid unnecessary synchronous database operations.

---

# Async

Database operations should generally use asynchronous EF Core methods:

```csharp
ToListAsync()
FirstOrDefaultAsync()
FindAsync()
SaveChangesAsync()
```

Do not introduce async complexity where it provides no value.

---

# Models vs ViewModels

Use ViewModels for forms when appropriate.

Example:

```text
Models/
    User.cs
    Wallet.cs
    Transaction.cs

ViewModels/
    LoginViewModel.cs
    RegisterViewModel.cs
    SendMoneyViewModel.cs
    ProfileViewModel.cs
```

Do not expose database entities directly when a form requires different fields.

---

# What NOT to Add

Do not introduce the following unless explicitly requested:

* MediatR
* CQRS
* DDD
* Clean Architecture
* Event Sourcing
* Repository Pattern everywhere
* Unit of Work abstraction
* Microservices
* Redis
* Kafka
* RabbitMQ
* PostgreSQL
* Kubernetes
* JWT
* GraphQL
* React
* Vue
* Angular
* AutoMapper
* FluentValidation
* MassTransit

The project is intentionally a simple MVC application.

---

# Development Workflow

Before implementing a feature:

1. Understand the existing MVC structure.
2. Check whether an existing service can handle the logic.
3. Add or modify a ViewModel if needed.
4. Implement business logic in a Service.
5. Keep the Controller thin.
6. Update the Razor View.
7. Test the functionality.
8. Verify the application still builds.

---

# Feature Example

For sending money:

```text
SendMoneyView
      ↓
TransactionsController
      ↓
TransactionService
      ↓
WalletService
      ↓
AppDbContext
      ↓
SQLite
```

The Controller should not directly manipulate balances.

---

# Database Migrations

Use Entity Framework Core migrations.

Example:

```bash
dotnet ef migrations add InitialCreate
```

Apply:

```bash
dotnet ef database update
```

If Docker handles database initialization, ensure the database is available before the application attempts to use it.

---

# Testing

For the MVP, prioritize manual functional testing.

At minimum verify:

### Authentication

* Register
* Login
* Logout
* Invalid credentials

### Wallet

* Balance display
* Deposit
* Withdrawal
* Transfer

### Transactions

* Transaction creation
* Transaction history
* Filtering

### Admin

* Admin access
* Normal user cannot access admin
* User activation/deactivation

### Security

* Unauthenticated users cannot access private pages
* Normal users cannot access admin pages

---

# Git

Use clear commit messages.

Examples:

```text
feat: add wallet dashboard
feat: add transaction service
fix: prevent negative wallet balance
feat: add admin user management
style: improve wallet layout
refactor: move transfer logic to service
```

---

# Implementation Rule

When adding functionality, first ask:

> Can this be implemented simply using the existing MVC + Services structure?

If yes, do that.

Do not introduce a new architectural layer unless there is a concrete reason.

---

# Final Rule

The project should remain understandable to a developer who opens the repository for the first time.

Prefer:

```text
Simple Controller
        ↓
Service
        ↓
DbContext
```

over a chain of unnecessary abstractions.

This is a **professional MVP**, not an enterprise architecture demonstration.
