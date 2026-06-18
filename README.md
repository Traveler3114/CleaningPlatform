# Internal Cleaning Operations Platform

A centralized operations platform for cleaning service businesses that combines CRM, booking management, workforce coordination, scheduling, SOP execution, invoicing, reporting, and customer operations into a single system.

---

## Overview

This platform replaces fragmented tools and spreadsheets with one integrated operational system.

The architecture follows a strict backend-driven approach:
- Backend contains all business logic
- Frontends are lightweight clients
- Centralized API powers all applications
- Operational data is managed from one source of truth

The project currently includes:
- ASP.NET Core 10.0 Web API
- Static HTML/JS admin dashboard
- Static HTML/JS customer booking site
- Static HTML/JS customer portal
- SQL Server database

---

## System Architecture

```
                    +-------------------------+
                    |  Customer Website       |
                    |  (HTML/CSS/JS)          |
                    +----------+--------------+
                               |
                               v
+-------------------+    +---------------------+
|  Customer Portal  |    |  ASP.NET Core API   |
|  (HTML/CSS/JS)    +--->+  Business Logic     |
+-------------------+    |  (Managers)          |
                         +----------+----------+
                         +----------+----------+
                         |  Admin Dashboard    |
                         |  (HTML/CSS/JS)      |
                         +----------+----------+
                                    |
                                    v
                           +------------------+
                           |   SQL Server     |
                           |   Database       |
                           +------------------+
```

---

## Technology Stack

### Backend
- ASP.NET Core 10.0 Web API
- Entity Framework Core 10.0
- Microsoft SQL Server
- JWT Bearer authentication
- BCrypt password hashing
- Scalar/OpenAPI documentation

### Frontend
- Vanilla HTML/CSS/JavaScript
- Bootstrap 5
- jQuery
- jQuery Validation

### Key Libraries
- ClosedXML (Excel export)
- SendGrid (email)

---

## Project Structure

```
CleaningPlatformAPI/
├── Program.cs                 # App startup, DI, middleware, error handling
├── Authorization/             # Custom permission handler
├── Common/                    # Shared types (AppException, PermissionKeys, RoleNames, SqlHelper)
├── Models/                    # Paginated<T>
├── Contracts/                 # DTOs (request/response types)
├── Controllers/               # REST API endpoints (16 controllers)
├── Data/                      # EF Core DbContext
├── Entities/                  # EF Core entity classes (29 files)
├── Enums/                     # Enumerations (BookingServiceType, BookingStatus)
├── Extensions/                # Extension methods (ClaimsPrincipal)
├── Managers/                  # Business logic (15 managers)
├── Mapping/                   # Static DTO mappers
├── Services/                  # External services (EmailService)
└── wwwroot/                   # Static frontend files
    ├── admin/                 # Admin dashboard
    ├── portal/                # Customer portal
    ├── public/                # Customer booking site
    └── lib/                   # Vendored libraries (Bootstrap, jQuery)
```

---

## Main Modules

### CRM & Client Management
- Client profiles with contacts and sites
- Multi-type clients (OneTime, RepeatIndividual, RepeatBusiness)
- Service catalog with 26 predefined services

### Booking & Scheduling
- Multi-type bookings (vehicle, site-based, boat)
- Time-slot availability with capacity management
- Weekly schedule templates + date overrides
- Full booking lifecycle: creation → assignment → execution → invoicing

### Workforce Management
- Employee management with roles and permissions
- Employee assignments to bookings
- Job status tracking

### SOP & Task Execution
- SOP templates with checklist items
- Checklist execution per booking assignment
- Proof-of-service tracking

### Financial Operations
- Invoice generation from bookings
- Payment recording and tracking
- Invoice status workflow (Draft → Sent → Paid / Overdue / WrittenOff)
- Financial reporting

---

## API Design

All endpoints follow a consistent pattern:
- Base route: `/api/{plural-noun}`
- All controllers have `[ApiController]`, `[Route]`, and class-level `[Authorize]`
- Action-level authorization via granular permission policies
- Error responses follow RFC 7807 `ProblemDetails`: `{ type, title, status, detail, code }`
- Success responses return data directly (no envelope)
- Global exception handler returns consistent `ProblemDetails`

---

## Authentication

- **Admin**: Username/password login → JWT token with role and permission claims
- **Portal**: Email-based magic link → JWT session token
- Both flows use the same token infrastructure

---

## Database

- Schema defined and maintained via SQL script (`docs/cleaning_platform.sql`)
- No EF Core migrations
- Entity classes reflect the SQL schema
- SQL views provide consolidated read-only data for reporting

---

## Business Rules

- All validation and business rules in backend managers only
- Working hours configurable per day of week
- Slot capacity with date/hour overrides
- Capacity = slot capacity - existing bookings
- Valid booking requires: within working hours, capacity available, slot not blocked

---

## Frontend Applications

### 1. Customer Booking Website (`wwwroot/public/`)
Public booking interface for customers to view availability and create bookings.

### 2. Admin Dashboard (`wwwroot/admin/`)
Internal management interface for all operations, built with static HTML/JS + Bootstrap.

### 3. Customer Portal (`wwwroot/portal/`)
Customer-facing portal for viewing bookings, invoices, and managing profile.

---

## Development Philosophy

- Reliability and maintainability
- Strict separation of concerns
- Backend as single source of truth
- API-first approach
- Granular permission-based access control

---

## License

Private internal business software. All rights reserved.
