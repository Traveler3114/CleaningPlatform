# Project context

## Stack
- **Language**: C# 12+ (backend API), HTML/CSS/JavaScript (frontend)
- **Framework**: ASP.NET Core 10.0 Web API
- **Database**: Microsoft SQL Server via Entity Framework Core 10.0
- **Auth**: JWT Bearer + policy-based authorization
- **Password hashing**: BCrypt.Net-Next
- **Email**: SendGrid
- **Excel export**: ClosedXML

## Project structure

```
CleaningPlatformAPI/
├── Program.cs                 # Startup, DI, middleware, global error handler
├── Authorization/             # PermissionHandler, PermissionRequirement
├── Common/                    # OperationResult<T>, PermissionKeys, RoleNames, PagedResult
├── Contracts/                 # DTOs (request/response records/classes)
├── Controllers/               # REST API endpoints
├── Data/                      # AppDbContext
├── Entities/                  # EF Core entity classes
├── Enums/                     # BookingServiceType, BookingStatus
├── Extensions/                # ClaimsPrincipalExtensions
├── Managers/                  # All business logic
├── Mapping/                   # Static DTO mappers
├── Services/                  # EmailService
└── wwwroot/                   # Static frontend
    ├── admin/                 # Admin dashboard
    ├── portal/                # Customer portal
    ├── public/                # Customer booking site
    └── lib/                   # Vendored Bootstrap, jQuery
```

---

## ⛔ Off-Limits — Do Not Touch Without Explicit Human Instruction

These areas must not be modified under any circumstances without a human explicitly requesting it.
They require careful reasoning across the full system and are easy to break silently.

| Area | Why |
|---|---|
| Security stamp validation | Touches the JWT auth pipeline — easy to lock all users out |
| Overbooking isolation level in `BookingManager` | Touches transaction behavior — requires load testing to validate |
| Biweekly parity anchor logic in `RecurringScheduleManager` | Subtle stateful time-based logic — could silently shift existing schedules |
| Any schema change beyond the `DateOverride` `EndHour` constraint | Schema changes need human review and coordinated migration |
| Invoice payment status logic | No correction endpoint is an intentional design decision |
| `Client.IsActive` | Clients are permanent once created — do not add a deactivation endpoint |

---

## Conventions — Code Style

### Namespaces
Always file-scoped: `namespace CleaningPlatformAPI.X.Y;`
- Never use block-scoped `namespace X.Y { }`

### Constructors
Single-line block body for simple field assignments:
```csharp
public BookingManager(AppDbContext db) { _db = db; }
public BookingController(BookingManager bookingManager, SopManager sopManager) { _bookingManager = bookingManager; _sopManager = sopManager; }
```
Use multi-line only when the constructor contains logic beyond assignments (e.g., reading config sections).

### Collection initialization
Use `[]` collection expressions everywhere:
```csharp
private List<AvailabilityResponse> slots = [];
public ICollection<Booking> Bookings { get; set; } = [];
return [];
```
Avoid `new List<T>()` in new code.

### Null checks
Use `is null` / `is not null` pattern:
```csharp
if (user is null) return ...;
if (user is not null) ...
```
Avoid `== null` / `!= null` except in lambda expressions where pattern matching isn't available.

### Private fields
```csharp
private readonly AppDbContext _db;
private readonly BookingManager _bookingManager;
```

### String defaults
```csharp
public string Name { get; set; } = string.Empty;
```
Never `= null!` or `= ""`.

### Using directives
Order: `System.*` → `Microsoft.*` → third-party → project (`CleaningPlatformAPI.*`).

### Async pattern
- All DB access and business logic is fully async
- `CancellationToken ct = default` as the **last** parameter on all async methods
- Always pass `ct` to EF Core methods

### Error handling
- **Global exception handler** in `Program.cs` catches and logs all unhandled exceptions, returns `OperationResult<string>.Fail(message)`
- **Transaction catch blocks**: catch → rollback → rethrow (never swallow)
- **Controllers never catch exceptions** — let them bubble to the global handler
- **Never silently swallow exceptions** — if you catch, you must log or rethrow

### TryParse over Parse
```csharp
// Good
return int.TryParse(claim, out var id) ? id : 0;

// Bad
return int.Parse(claim);
```

---

## Conventions — Architecture

### Manager pattern
- All business logic lives in **Manager** classes — never in controllers
- Managers inject `AppDbContext` and/or other managers via constructor
- Managers are concrete classes (no interfaces)
- Manager naming: `{Domain}Manager` (e.g., `BookingManager`, `InvoiceManager`)

### Controllers
- Always annotated: `[ApiController]`, `[Route("api/{plural-noun}")]`, `[Authorize]`
- Route matches controller name: `EmployeeController` → `api/employees`
- Action-level authorization: `[Authorize(Policy = PermissionKeys.BookingsView)]`
- Always return `ActionResult<OperationResult<T>>`
- Thin: receive input → call manager → forward result
- Never contain business logic

### OperationResult pattern
All manager methods return `OperationResult<T>`:
```csharp
return OperationResult<T>.Ok(data);
return OperationResult<T>.Fail("error message");
```
Controllers forward directly:
```csharp
return result.Success ? Ok(result) : NotFound(result);
```

### Status codes
| Code | When |
|------|------|
| 200 | Success (via `Ok(result)`) |
| 400 | Bad request (via `BadRequest(result)`) |
| 404 | Not found (via `NotFound(result)`) |
| 422 | Validation failure (via `UnprocessableEntity(result)`) |
| 401 | Unauthorized (via `Unauthorized(result)`) |
| 403 | Forbidden (via `Forbid()`) |

### DTOs
- Defined in `Contracts/` folder, organized by domain (e.g., `BookingContract.cs`, `InvoiceContract.cs`)
- Request DTOs: `{Action}{Domain}Request` (e.g., `CreateBookingRequest`)
- Response DTOs: `{Domain}Response` (e.g., `BookingResponse`)
- Use `record` or `class` consistently per file; prefer `record` for simple DTOs

### Mappers
- Static classes in `Mapping/` folder: `{Domain}Mapper` (e.g., `BookingMapper`)
- Manual field mapping methods (no AutoMapper)
- Names: `ToResponse()`, `ToEntity()`, `ToDto()` depending on direction

---

## Conventions — Validation

### Validate in the manager, not the database
If a field has a restricted set of allowed values, validate it explicitly in the manager and return `OperationResult.Fail` with a clear message. Never rely on a SQL constraint violation to be the user-facing error. The database constraint is a safety net, not the validation layer.

```csharp
// Good
var validTypes = new[] { "Vehicle", "SiteBased", "Boat", "Generic" };
if (!validTypes.Contains(dto.ServiceType?.Trim()))
    return OperationResult<T>.Fail("ServiceType must be one of: Vehicle, SiteBased, Boat, Generic.");

// Bad — lets the SQL constraint throw a generic error
entity.ServiceType = dto.ServiceType;
await _db.SaveChangesAsync(ct);
```

This applies consistently across all managers. If `ServiceCatalogManager` validates a field, `SopManager` working with the same field must also validate it.

---

## Conventions — Data Integrity

### Hard delete is never used on operational records
Bookings, assignments, invoices, and their related records are **cancelled or deactivated**, never deleted. Deleting operational records destroys the audit trail and silently removes data that employees, finance, and management may depend on.

- **Bookings**: set `Status = Cancelled`
- **Assignments**: remove only if booking is still Pending or Confirmed
- **Clients**: permanent — no deactivation endpoint exists and none should be added
- **Sites / SOPs**: set `IsActive = false`
- **Hard delete is only permitted** for configuration records that have never been used (e.g., a role with no users assigned)

When cancelling a series of records (e.g., future recurring bookings on schedule update), always cancel — never `RemoveRange`.

### SOP templates are never deleted
SOP templates use activate/deactivate only. There is no delete endpoint and none should be added. This is intentional — templates may be referenced by historical bookings and must be preserved.

- `IsActive = true` → template is available to assign to new bookings
- `IsActive = false` → template is hidden from assignment but preserved for history
- To manage SOPs: create new, edit existing, or toggle active status via `PUT /api/sops/{id}/active`

### Duplicate-safe inserts on junction tables
Always check for an existing record before inserting into a junction table. Never rely on the unique constraint to catch duplicates at the database level — that produces an unhandled exception rather than a clean result.

```csharp
// Good
var exists = await _db.BookingSopAssignments
    .AnyAsync(a => a.BookingId == bookingId && a.SopTemplateId == templateId, ct);
if (exists)
    return OperationResult<T>.Fail("Already assigned.");

// Bad — lets the unique constraint throw
_db.BookingSopAssignments.Add(new BookingSopAssignment { ... });
await _db.SaveChangesAsync(ct);
```

This applies to: `BookingSopAssignments`, `BookingAssignments`, `InvoiceBookings`, and any other junction table with a unique constraint.

### Security stamp — known gap, do not remove
The `security_stamp` claim is included in admin JWTs and is rotated correctly on password change and password reset. Incoming requests do not currently validate the stamp against the database — this is a known gap documented for future work.

Do not remove the stamp from token creation. Do not add stamp validation without explicit human instruction — it touches the auth pipeline and requires careful implementation.

### DateOverride and WeeklySchedule both use 24 as maximum EndHour
Both `WeeklySchedule` and `DateOverride` use `EndHour = 24` to represent end of day. The `AvailabilityManager` loop treats `EndHour` as an exclusive upper bound — `EndHour = 24` means the last bookable slot is `23:00`. Do not change either cap back to 23.

---

## Conventions — Mappers

### Every field on the source must be explicitly accounted for
When writing a mapper method or adding a new overload, go through every field on the source type and either map it to the response or leave an explicit comment explaining why it is intentionally excluded.

Silent omissions — where a field exists on the source, is populated by the query, but is simply absent from the mapper — are not acceptable.

```csharp
// Good
public static BookingResponse ToResponse(BookingView view) => new()
{
    ServiceType  = view.ServiceType,   // must map — not string.Empty
    LicensePlate = view.LicensePlate,  // must map even for non-vehicle bookings (nullable)
    // SiteCity intentionally excluded from list response — available in detail endpoint only
};

// Bad
public static BookingResponse ToResponse(BookingView view) => new()
{
    ServiceType  = string.Empty,       // silently drops the field
    // LicensePlate, CarModel, BoatType, LengthMeters — silently omitted
};
```

When a mapper has multiple overloads, they must return consistent field coverage for fields that both sources populate.

---

## Conventions — Employees

### What is updatable on an employee
The following fields are updatable via `PUT /api/employees/{id}` with `UsersEdit` permission:
- `Role` (by role name string)
- `HourlyRate`
- `MaxJobsPerDay`
- `EmployeeCode`

The following are managed by separate dedicated endpoints and must not be included in the general update:
- `IsActive` → `PUT /api/employees/{id}/toggle`
- `PasswordHash` → `POST /api/auth/reset-password` or `POST /api/auth/change-password`

An employee must not be able to change their own role. The manager must check that the requesting user id differs from the target id before applying a role change.

---

## Conventions — API Routes

### Controller route naming
| Controller | Route |
|---|---|
| `BookingController` | `api/bookings` |
| `ClientController` | `api/clients` |
| `EmployeeController` | `api/employees` |
| `InvoiceController` | `api/invoices` |
| `AuthController` | `api/auth` |
| `PortalAuthController` | `api/portal` |
| `ScheduleController` | `api/schedule` |
| `ServiceCatalogController` | `api/services` |
| `SopController` | `api/sops` |
| `RoleController` | `api/roles` |
| `AvailabilityController` | `api/availability` |
| `ReportingController` | `api/reports` |
| `KanbanController` | `api/kanban` |
| `DateOverrideController` | `api/overrides` |
| `AssignmentController` | `api/assignments` |

### Route parameter naming
Use camelCase with type constraints: `{bookingId:int}`, `{id:int}`.

### Fallback route scope
`MapFallbackToFile` and any fallback handler must only apply to non-API routes. Any request path starting with `/api/` that does not match a controller action must return `404` with a JSON `OperationResult` body — never an HTML page. Serving HTML for unmatched API paths hides bugs from API clients and monitoring tools.

---

## Conventions — Permission System

### Permission keys
Defined in `Common/PermissionKeys.cs`. Format: `{domain}.{action}`:
```csharp
public const string BookingsView = "bookings.view";
public const string InvoicesCreate = "invoices.create";
```

### Key management
- All keys must be in both `All` array and `Meta` dictionary
- Static constructor validates sync at startup (throws if out of sync)
- `Meta` provides display name, description, and category for UI
- Every permission key that is seeded to a role **must** be used by at least one `[Authorize(Policy = ...)]` attribute. Seeding a key that no controller checks is dead configuration — either add the policy check or remove the key from the seed.

### Role names
Defined in `Common/RoleNames.cs`: `Owner`, `Admin`, `Dispatcher`, `Employee`, `Finance`

### Authorization
- `Owner` role bypasses all permission checks (via `PermissionHandler`)
- Non-Owner users require specific permission claims
- Permission claims added to JWT at login

---

## Conventions — Database

### SQL-first approach
- Schema defined in `docs/cleaning_platform.sql`
- No EF Core migrations (`dotnet ef database update` is never used)
- Entity classes reflect the SQL schema
- Schema changes: update SQL script first, then entities + AppDbContext
- `cleaning_platform.sql` is the **canonical schema**. `cleaning_platform_seed.sql` is for test/dev seeding only. When the schema changes, both files must be updated together and kept in sync.

### Keyless entities
SQL views are mapped as keyless entities:
```csharp
modelBuilder.Entity<BookingView>()
    .HasNoKey()
    .ToView("vw_Bookings");
```

---

## Conventions — Frontend

### API client
Shared client in `wwwroot/admin/js/admin-api.js`:
```javascript
const API_BASE = '/api';
async function apiFetch(endpoint, options = {}) {
    const token = getToken();
    const response = await fetch(`${API_BASE}${endpoint}`, { ... });
    if (response.status === 401) { logout(); throw ...; }
    return response.json();
}
```

### JWT storage
- Token stored in `localStorage` as `accessToken`
- JWT payload decoded client-side for permission checks
- `admin-common.js` populates user pill and permission cache on every page

### Folder structure
- `wwwroot/admin/js/` — admin-specific JS
- `wwwroot/admin/css/` — admin styles
- `wwwroot/portal/js/` — portal JS
- `wwwroot/public/js/` — customer booking site JS

---

## Conventions — Tests

### Integration tests must assert their preconditions
If a test requires data to exist, assert that it exists. Never use an early `return` to silently pass when the database is empty — that produces a vacuous green test that proves nothing.

```csharp
// Good
var all = await manager.GetAllAsync(new PaginationParams());
all.Items.Should().NotBeEmpty("seed data must exist for this test to be meaningful");

// Bad
var all = await manager.GetAllAsync(new PaginationParams());
if (all.Items.Count == 0) return; // silently passes, tests nothing
```

If the data genuinely may not exist in all environments, mark the test with `Skip` and a reason instead.

### Integration test actor/target must be different users
Any test that calls a method rejecting self-action (e.g., toggling your own account) must use two distinct users. Always assert that at least two users exist before relying on index-based selection.

```csharp
// Good
var all = await manager.GetAllUsersAsync();
all.Should().HaveCountGreaterThan(1, "need at least two users to test actor/target separation");
var target = all[0];
var actor = all.First(u => u.Id != target.Id);
```

---

## Conventions — General Rules

### What to avoid
- Avoid `== null` / `!= null` (use `is null` / `is not null`)
- Avoid block-scoped namespaces (use file-scoped)
- Avoid `new List<T>()` (use `[]`)
- Avoid auto-implemented mappers (map manually)
- Avoid putting logic in controllers (use managers)
- Avoid silent exception swallowing (log or rethrow)
- Avoid EF Migrations (use SQL scripts)
- Avoid magic string status/type fields — use enums with `HasConversion<string>()` instead
- Avoid relying on SQL constraint violations as the user-facing validation error
- Avoid hard deleting operational records (cancel or deactivate instead)
- Avoid deleting SOP templates — use activate/deactivate only
- Avoid early `return` in tests when data is missing (assert or skip)
- Avoid seeding permission keys that no controller policy actually checks
- Avoid serving HTML for unmatched `/api/` routes

### What to always do
- Always use `CancellationToken` as last parameter on async methods
- Always return `OperationResult<T>` from managers
- Always use `[ApiController]` + `[Route]` + `[Authorize]` on controllers
- Always use file-scoped namespaces
- Always use `string.Empty` for string defaults
- Always use `is null` / `is not null` for null checks
- Always throw if PermissionKeys.All and Meta are out of sync
- Always use enums for status and type fields with `HasConversion<string>()` in the DbContext — never use raw magic strings. Define the enum in `Enums/` and reference it by name in the entity and manager.
- Always validate restricted-value fields explicitly in the manager before saving
- Always account for every source field in every mapper method (map it or comment why not)
- Always check for existing records before inserting into junction tables
- Always keep `cleaning_platform.sql` and `cleaning_platform_seed.sql` in sync when schema changes
- Always check the off-limits list before modifying any sensitive area

### Naming summary
| Element | Convention | Example |
|---|---|---|
| Classes | PascalCase | `BookingManager` |
| Methods | PascalCase | `CreateBookingAsync` |
| Local variables | camelCase | `bookingList` |
| Private fields | `_camelCase` | `_db` |
| Parameters | camelCase | `bookingId` |
| Async methods | `{Verb}{Noun}Async` | `GetByIdAsync` |
| Route params | camelCase | `{bookingId}` |
| Permission keys | `{domain}.{action}` | `bookings.view` |
| DTO requests | `{Action}{Domain}Request` | `CreateBookingRequest` |
| DTO responses | `{Domain}Response` | `BookingResponse` |
| Controllers | `{Domain}Controller` | `BookingController` |
| Managers | `{Domain}Manager` | `BookingManager` |
| Mappers | `{Domain}Mapper` | `BookingMapper` |

### Blank lines
- Single blank line between methods
- No double blank lines anywhere
- Single blank line between using blocks and namespace declaration