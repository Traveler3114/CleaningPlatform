# Project context

## Stack
- **Language**: C# (backend Razor Pages & API), HTML/CSS/JavaScript (customer booking UI)
- **Framework**: ASP.NET Core 10.0 (API + Razor Pages), .NET 10.0
- **Database**: Microsoft SQL Server
- **Package manager**: NuGet

## Dependencies
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- (JWT authentication, policy-based authorization – from `Program.cs` and `TokenManager`)

## Conventions
- All core business logic lives in dedicated **Manager** classes (e.g., `BookingManager`, `InvoiceManager`, `ClientManager`) – never in controllers or pages.
- Razor Pages and controllers act as presentation layers: they receive input, call managers, and return results.
- **Managers themselves return `OperationResult<T>` (or `OperationResult` without data)**. They use `OperationResult.Ok(data)` for success and `OperationResult.Fail("error message")` for failure. Controllers simply forward the result.
- Controllers always return `Task<OperationResult<T>>` (or `Task<OperationResult>` for void operations).
- DTOs are used throughout for data transfer; entities are strictly internal to the data layer.
- Roles and permissions are granular (`Owner`, `Admin`, `Dispatcher`, `Employee`, `Finance`) and enforced via custom authorization handlers.
- The database schema uses proper constraints, unique indexes, and referential integrity (SQL Server).
- Seed data is included for development and demonstration (employees, clients, bookings, invoices, etc.).
- All database access, business logic, token generation, and other operations are fully asynchronous (async/await).

## Authentication
- **API authentication**: Users log in with username/password to receive a JWT token (issued by `TokenManager`).
- **Razor Pages authentication**: Cookie-based. The custom `SecurityStampValidator` middleware validates the user’s security stamp on each request and rejects sessions if the stamp has changed.
- Both flows use the same `AuthManager` for credential validation and role/permission claim generation.

## Database approach
- **The database is defined and maintained via the SQL script (`cleaning_platform.sql`), not by EF Core Migrations.**
- Entity classes (`Entities/*.cs`) are written to reflect the SQL schema – they follow the script, never the reverse.
- We do **not** use `dotnet ef database update` or any migration workflow.
- If a database change is needed, the SQL script is updated first, then the corresponding C# entity classes and the `AppDbContext` configuration are adjusted to match.
- **Important**: Neither the SQL script nor entity classes should be modified unless explicitly requested.

## Database views
- `vw_Bookings` and `vw_InvoiceSummary` are SQL views that provide consolidated read-only data for the booking list and invoice summary.
- In EF Core, they are mapped as **keyless entities** (`.ToView()`, `.HasNoKey()`) and are never written to directly.
- They help simplify reporting without duplicating complex queries in C#.

## Customer booking UI
- The customer-facing booking widget is a static HTML/JS application located in `wwwroot/` (`index.html`, `js/app.js`, `css/customer.css`).
- It provides a multi-step booking flow that calls the public API endpoints (`/api/availability`, `/api/bookings`, etc.).
- CORS is configured to allow the separate frontend to communicate with the API.

## Key files / folders
- `CleaningPlatformAPI/Program.cs` – Service setup, DB context, CORS, authentication, authorization, routing.
- `CleaningPlatformAPI/Data/AppDbContext.cs` – EF Core `DbContext` with entity configurations and view mappings.
- `CleaningPlatformAPI/Managers/` – All business logic (e.g., `BookingManager.cs`, `InvoiceManager.cs`, `AvailabilityManager.cs`).
- `CleaningPlatformAPI/Controllers/` – REST API endpoints for bookings, clients, availability, invoices, roles, etc.
- `CleaningPlatformAPI/Pages/` – Razor Pages admin dashboard (index, clients, bookings, invoices, users, roles, services, schedule).
- `CleaningPlatformAPI/Entities/` – Entity Framework entity classes mapped to SQL Server tables.
- `CleaningPlatformAPI/Dtos/` – Data transfer objects used by the API and pages.
- `CleaningPlatformAPI/wwwroot/` – Static files: customer booking UI (`index.html`, `js/app.js`, `css/customer.css`).
- `cleaning_platform.sql` – Full SQL schema (recreatable) with seed data, views, and constraints.
- `internal_cleaning_operations_platform.md` – Product brief describing the platform’s vision, workflows, data model, and roadmap.

## Project overview
This is an **internal cleaning operations platform** that unifies CRM, booking management, workforce coordination, SOP checklists, invoicing, payment tracking, and financial reporting for a cleaning services business. It supports:

- **Client & site management** (contacts, billing terms, multiple service locations)
- **Multi-type bookings**: vehicle cleaning, site-based cleaning, boat cleaning – with specialized details per type
- **Service catalog** with 26 predefined services across categories (stairs, offices, fleet, carwash, special, boats, upsell)
- **Employee management** with roles, permissions, hourly rates, and capacity limits
- **Scheduling**: weekly capacity template + date overrides, integrated into booking availability checks
- **Full booking life cycle**: creation → assignment → on-site execution (with check‑in/out, SOP checklists) → completion → invoicing
- **Invoicing and payments**: invoices generated from bookings, payment recording, status tracking (Draft → Sent → PartiallyPaid → Paid / Overdue / WrittenOff)
- **Views** (`vw_Bookings`, `vw_InvoiceSummary`) for consolidated reporting
- **Customer-facing booking UI** (static HTML/JS) that calls the API

The backend enforces all business rules (working hours, capacity, duplicate checks, valid status transitions). Frontends are thin clients that only display data and call the API.

## Notes
- The system replaces a previous vehicle-only cleaning platform; the codebase has been fully expanded to a general cleaning operations platform.
- All seed data is realistic for the Croatian market (service names, employee names, license plates, etc.).
- The project targets .NET 10.0 (preview) and uses the latest EF Core conventions.