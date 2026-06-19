# AGENTS.md — CleaningPlatform

## Stack
ASP.NET Core 10.0 Web API, EF Core 10.0, SQL Server, xUnit v3, FluentAssertions 7.x, Playwright

## Commands
```powershell
# Build
dotnet build CleaningPlatformAPI\CleaningPlatformAPI.csproj

# Unit tests (pure C#, no DB)
dotnet test CleaningPlatform.Tests\Unit

# Integration tests (needs SQL Server with seed data)
dotnet test CleaningPlatform.Tests\Integration

# E2E Playwright tests (needs API running + seeded DB)
npx playwright test --config CleaningPlatform.Tests\Playwright\playwright.config.ts
```

## Build quirk
`build/generate-i18n.mjs` runs automatically before every build (csproj target) — generates `wwwroot/i18n/` + `wwwroot/js/i18n-data.js`.

## Database (SQL-first, no EF migrations)
- `docs/cleaning_platform.sql` is canonical schema
- Schema change: update `.sql` → update entity classes → update `AppDbContext`
- `db.Database.EnsureCreated()` runs in dev only (`Program.cs:282`)
- SQL views → keyless entities with `.HasNoKey().ToView("vw_...")`
- `cleaning_platform_seed.sql` used for test/dev seeding; keep both `.sql` files in sync
- `DateOverride` and `WeeklySchedule` use `EndHour = 24` (exclusive upper bound, last slot 23:00)

## Solution layout
```
CleaningPlatform.slnx
├── CleaningPlatformAPI/           # ASP.NET Core Web API
│   ├── Program.cs                 # Startup, DI, middleware, global error handler
│   ├── Authorization/             # PermissionHandler, PermissionRequirement
│   ├── Common/                    # AppException, PermissionKeys, RoleNames, SqlHelper
│   ├── Controllers/               # REST endpoints (thin)
│   ├── Data/AppDbContext.cs       # EF Core DbContext
│   ├── Entities/                  # EF Core entity classes
│   ├── Managers/                  # All business logic
│   ├── Mapping/                   # Static DTO mappers (no AutoMapper)
│   ├── Contracts/                 # Request/response DTO records
│   └── wwwroot/                   # Static frontends
│       ├── admin/                 # Admin dashboard
│       ├── portal/                # Customer portal
│       └── public/                # Customer booking site
├── CleaningPlatform.Tests/Unit/   # Unit tests
├── CleaningPlatform.Tests/Integration/  # Integration tests
└── CleaningPlatform.Tests/Playwright/   # E2E tests (separate Node project)
```

## Architecture
- **Manager pattern**: all logic in concrete Manager classes (no interfaces), injected via `AddScoped<XManager>()`
- **Controllers**: `[ApiController]` + `[Route("api/{plural}")]` + `[Authorize]` — thin, never contain business logic
- **DTOs**: records in `Contracts/`, named `{Action}{Domain}Request` / `{Domain}Response`
- **Mappers**: static classes in `Mapping/`, manual field mapping (no AutoMapper) — every source field must be mapped or explicitly commented as excluded
- **Frontend**: vanilla HTML/CSS/JS + Bootstrap 5 + jQuery, served from `wwwroot/`
- **Entrypoint**: `Program.cs:286` — `app.Run()`

## Error handling
- Managers throw `AppException(code, message, statusCode)` for known errors
- Global exception handler in `Program.cs:157` catches all → `ProblemDetails` (RFC 7807): `{ type, title, status, detail, code }`
- Controllers never catch exceptions
- Never swallow exceptions — catch only to log or rethrow

## Validation
- Validate restricted-value fields explicitly in managers before saving — never rely on SQL constraints as user-facing errors
- Check for existing records before inserting into junction tables (duplicate-safe)

## Data integrity
- **No hard delete** on operational records: cancel bookings (`Status = Cancelled`), deactivate clients/SOPs (`IsActive = false`)
- SOP templates: activate/deactivate only — no delete endpoint
- Hard delete only permitted for unused config records (e.g., role with no users)

## Off-limits — do not modify without explicit instruction
| Area | Why |
|------|-----|
| Security stamp validation middleware | JWT auth pipeline — easy to lock all users out |
| Overbooking isolation level in `BookingManager` | Transaction behavior — needs load testing |
| Biweekly parity anchor in `RecurringScheduleManager` | Subtle time logic — could silently shift schedules |
| Schema changes beyond `DateOverride.EndHour` cap | Needs human review + coordinated migration |
| Invoice payment status logic | No correction endpoint — intentional |
| `Client.IsActive` | Clients are permanent — no deactivation endpoint exists |

## Code style
- **File-scoped namespaces**: `namespace CleaningPlatformAPI.X.Y;` (no block-scoped)
- **Null checks**: `is null` / `is not null` (not `== null` / `!= null`)
- **Collections**: `[]` expressions (not `new List<T>()`)
- **String defaults**: `= string.Empty` (not `= null!` or `= ""`)
- **Private fields**: `_camelCase`
- **Constructors**: single-line block for simple field assignments
- **Using order**: `System.*` → `Microsoft.*` → third-party → `CleaningPlatformAPI.*`
- **Async**: `CancellationToken ct = default` as last param on all async methods
- **Enums**: `HasConversion<string>()` on status/type fields — no raw magic strings
- **Single blank line** between methods, no double blank lines, blank line between using blocks and namespace

## Naming
| Element | Convention | Example |
|---------|-----------|---------|
| Classes/Methods | PascalCase | `BookingManager`, `CreateBookingAsync` |
| Local vars | camelCase | `bookingList` |
| Private fields | `_camelCase` | `_db` |
| Parameters | camelCase | `bookingId` |
| Route params | camelCase | `{bookingId:int}` |
| Permission keys | `{domain}.{action}` | `bookings.view` |
| DTO requests | `{Action}{Domain}Request` | `CreateBookingRequest` |
| DTO responses | `{Domain}Response` | `BookingResponse` |
| Controllers | `{Domain}Controller` | `BookingController` |
| Managers | `{Domain}Manager` | `BookingManager` |
| Mappers | `{Domain}Mapper` | `BookingMapper` |
| Paginated | `Paginated<T>` | `Paginated<BookingResponse>` |

## Permission system
- Keys in `Common/PermissionKeys.cs`: format `{domain}.{action}`, both in `All[]` and `Meta` dict
- Static constructor validates `All` ↔ `Meta` sync at startup (throws if mismatch)
- Every seeded key must be used by at least one `[Authorize(Policy = ...)]`
- Roles: `Owner` (bypasses all checks), `Admin`, `Dispatcher`, `Employee`, `Finance`

## API routes
| Controller | Route |
|-----------|-------|
| `AuthController` | `api/auth` |
| `PortalAuthController` | `api/portal` |
| `BookingController` | `api/bookings` |
| `ClientController` | `api/clients` |
| `EmployeeController` | `api/employees` |
| `InvoiceController` | `api/invoices` |
| `ScheduleController` | `api/schedule` |
| `DateOverrideController` | `api/overrides` |
| `ServiceCatalogController` | `api/services` |
| `SopController` | `api/sops` |
| `RoleController` | `api/roles` |
| `AvailabilityController` | `api/availability` |
| `ReportingController` | `api/reports` |
| `KanbanController` | `api/kanban` |
| `AssignmentController` | `api/assignments` |

Unmatched `/api/*` routes return 404 JSON `ProblemDetails` — never HTML.

## Frontend conventions
- Token in `localStorage` as `accessToken`
- Shared client: `wwwroot/admin/js/admin-api.js` — `apiFetch(endpoint, options)`
- Success responses return data directly (no envelope), errors are `ProblemDetails` shape

## Test conventions
- **Integration tests** use `TransactionScope` auto-rollback via `TestBase`
- Assert preconditions exist — never early-return to silently pass
- Self-action rejection tests: use two distinct users, assert ≥ 2 exist
- **Unit tests**: `CleaningPlatform.Tests.Unit.csproj` references the API project
- **Integration tests**: `CleaningPlatform.Tests.Integration.csproj` — need SQL Server + seeded `CleaningPlatformDB_Test`
- **Playwright E2E**: `CleaningPlatform.Tests.Playwright/` — need API running + seeded DB; config in `.env`

## Employees
- Updatable via `PUT /api/employees/{id}`: `Role`, `HourlyRate`, `MaxJobsPerDay`, `EmployeeCode`
- Separate endpoints: `IsActive` → `PUT /api/employees/{id}/toggle`; password → `POST /api/auth/reset-password`
- Employee must not change their own role — manager checks actor ≠ target
