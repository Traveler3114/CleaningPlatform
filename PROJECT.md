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

## Conventions — General Rules

### What to avoid
- Avoid `== null` / `!= null` (use `is null` / `is not null`)
- Avoid block-scoped namespaces (use file-scoped)
- Avoid `new List<T>()` (use `[]`)
- Avoid auto-implemented mappers (map manually)
- Avoid putting logic in controllers (use managers)
- Avoid silent exception swallowing (log or rethrow)
- Avoid EF Migrations (use SQL scripts)

### What to always do
- Always use `CancellationToken` as last parameter on async methods
- Always return `OperationResult<T>` from managers
- Always use `[ApiController]` + `[Route]` + `[Authorize]` on controllers
- Always use file-scoped namespaces
- Always use `string.Empty` for string defaults
- Always use `is null` / `is not null` for null checks
- Always throw if PermissionKeys.All and Meta are out of sync

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
