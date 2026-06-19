# AGENTS.md — Cleaning Platform

**Canonical instructions:** `PROJECT.md` (read it first). This file only adds what PRO-JECT.md does not cover or what is easy to miss.

## Commands (run from repo root)

```powershell
dotnet test CleaningPlatform.Tests\Unit
dotnet test CleaningPlatform.Tests\Integration
npx playwright test --config CleaningPlatform.Tests\Playwright\playwright.config.ts
dotnet run --project CleaningPlatformAPI
```

No formatters, linters, or pre-commit hooks are configured.

## Architecture

- **.NET 10.0** Web API, solution uses new `.slnx` format (not `.sln`)
- **SQL-first schema** — no EF Core migrations. Schema: `docs/cleaning_platform.sql`. Seed: `docs/cleaning_platform_seed.sql`. Keep both in sync.
- **No AutoMapper** — manual static mappers in `Mapping/`
- **Pre-build codegen:** `node build/generate-i18n.mjs` runs before every build (parses `.resx` → `wwwroot/i18n/`). Mistakes here break the build.
- **All business logic in Managers** (concrete classes, no interfaces). Controllers are thin — never contain logic.
- **Error handling:** Managers throw `AppException(code, message, statusCode)` → global handler → RFC 7807 `ProblemDetails`. Controllers never catch.
- **JWT auth** with policy-based permissions. `Owner` role bypasses all checks.
- **Frontend:** Vanilla HTML/JS (Bootstrap 5, jQuery) in `wwwroot/{admin,portal,public}/`. No SPA framework.

## Testing quirks

- **xUnit v3** + **FluentAssertions**. Suppress warning xUnit1051.
- **Integration tests need a running SQL Server** with database `CleaningPlatformDB_Test`. Local: use `appsettings.Test.json`. CI spins up Azure SQL Edge.
- **Playwright tests** are in `CleaningPlatform.Tests/Playwright/` (separate `package.json`). Load `.env` from that directory. Server must be running.
- Integration tests must assert preconditions (never early-return on missing data).

## Off-limits (from PROJECT.md)

| Area | Why |
|------|-----|
| Security stamp validation | Auth pipeline — easy to lock all users out |
| Overbooking isolation level in `BookingManager` | Transaction behavior — needs load testing |
| Biweekly parity anchor in `RecurringScheduleManager` | Subtle time logic — could silently shift schedules |
| Invoice payment status logic | No correction endpoint by design |
| `Client.IsActive` | Clients are permanent — no deactivation endpoint |

## Conventions an agent would likely guess wrong

- **Private fields:** `_camelCase` (e.g., `_db`)
- **Null checks:** `is null` / `is not null` (not `== null`)
- **Collection init:** `[]` (not `new List<T>()`)
- **String defaults:** `string.Empty` (not `null!`)
- **CancellationToken:** always last parameter on async methods
- **Constructors:** single-line block body for simple assignments
- **DTOs:** `{Action}{Domain}Request` / `{Domain}Response` in `Contracts/`
- **Route params camelCase:** `{bookingId:int}`, `{id:int}`
- **Enums over magic strings:** Use `HasConversion<string>()` in DbContext
- **Hard delete forbidden** on operational records (cancel/deactivate instead)
- **Duplicate-safe junction inserts** — check existence before inserting
- **All mapper fields must be accounted for** — map or comment why excluded
- **`EndHour = 24`** is the exclusive upper bound (last slot is 23:00)
