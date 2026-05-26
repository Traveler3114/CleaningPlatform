# Code Review Summary — Cleaning Operations Platform

## Architecture & Design

The overall architecture is clean and consistent. The Manager pattern is applied uniformly, controllers are genuinely thin, all business logic lives in managers, and the `OperationResult<T>` wrapper keeps response handling predictable. The permission system is granular and the SQL-first approach is reasonable for this type of project. The codebase has clearly grown organically — the core booking and client modules are the most polished, SOP is solid, recurring schedules slightly less mature in edge case handling, and the portal feature has the thinnest coverage overall.

---

## Security Issues

- **JWT secret committed to repository.** `appsettings.json` contains the plaintext JWT secret. The same secret appears in the Playwright test helper `env.ts`. Should be in environment variables or a secrets manager.

- **Security stamp not validated on requests.** `TokenManager` includes a `security_stamp` claim in the JWT and both `ResetPasswordAsync` and `ChangePasswordAsync` rotate it correctly, but nothing validates the stamp on incoming requests. Existing tokens remain valid after a password change until natural expiry — up to 8 hours.

- **No rate limiting on anonymous booking endpoint.** `POST /api/bookings` is `[AllowAnonymous]` with no rate limiting, allowing unlimited client and booking creation.

- **Portal token not explicitly rejected from admin endpoints.** The `PortalOnly` policy restricts portal endpoints but there is no explicit `auth_type == "admin"` check before processing admin permissions. Portal tokens being tried against admin endpoints are rejected coincidentally rather than deliberately.

- **Magic link error messages slightly inconsistent.** `SendLink` correctly avoids email enumeration, but `ValidateToken` returns different error messages for expired vs invalid tokens, which is a minor inconsistency in the security model.

---

## Data Integrity Issues

- **No state machine on booking status.** `UpdateStatusAsync` allows any valid enum transition including `Cancelled → Completed`. The skipped test `UpdateStatusAsync_InvalidTransition_ReturnsFail` acknowledges this gap explicitly.

- **Invoice payment status never moves backward.** `RecordPaymentAsync` only transitions status toward `Paid`. If a payment is corrected or deleted directly in the database, the invoice status stays `Paid`. There is no payment deletion or correction endpoint in the API.

- **`EnsureServiceSopsAssignedAsync` duplicate insertion risk.** Called from both `CreateAdminBookingAsync` and `GenerateForScheduleAsync`, it inserts directly without checking for existing assignments, relying on the database unique constraint to catch duplicates. Duplicate calls throw a SQL exception rather than being handled gracefully.

- **`RecurringScheduleManager.UpdateAsync` hard deletes future bookings.** When a recurring schedule is updated, future pending bookings are `RemoveRange` deleted, silently cascade deleting any assignments, services, and SOPs attached to them. Assigned employees receive no notification. Cancelling rather than deleting would preserve the audit trail.

- **Invoice number gaps.** The SQL sequence used for invoice numbers is consumed even if the transaction rolls back, producing gaps that can cause confusion with accountants.

- **`ClientManager.CreateBookingAsync` hardcodes client type as `"Person"`.** All clients created through the public booking form are always `Person` regardless of what they might actually be.

---

## Business Logic Bugs

- **Biweekly parity anchor shifts over time.** `GetOccurrenceDates` anchors biweekly parity against `rangeStart`, which changes on every run since `RunAutoGenerateAsync` uses today's date. A biweekly schedule could start generating on different weeks over time. The anchor should be fixed to the source booking creation date.

- **`SopManager.GetBookingSopsAsync` only shows one employee's checklist responses.** The per-item checklist detail always shows responses from the lowest-ID assignment only. If a second assigned employee completes items, those completions are invisible in the detail view even though the aggregate count is correct. This causes a confusing mismatch where the count shows 5/5 complete but the checklist detail shows nothing checked.

- **`SopManager.DeleteTemplateAsync` returns `Success = true` for both deletion and deactivation.** The caller has no programmatic way to distinguish between the two outcomes without parsing the message string. Should either return different status values or include a flag like `wasDeactivated`.

- **`WeeklySchedule` allows `EndHour = 24` but `DateOverride` caps at 23.** You can configure a weekly schedule running until midnight but cannot replicate that with a date override. The `AvailabilityManager` loop uses `EndHour` as an exclusive upper bound so 24 means last slot is `23:00`, but date overrides can never achieve this.

- **`AvailabilityManager` timezone handling is fragile.** `date` from the HTTP request and `now` converted to Croatian time could disagree about whether today is today near midnight, causing past slots to be shown or future slots hidden for non-Croatian callers.

- **Overbooking protection is incomplete.** The double-check inside the transaction runs at `ReadCommitted` isolation level. Two concurrent requests could both see capacity available and both proceed to insert. `UPDLOCK` or `Serializable` isolation would be needed for genuine protection.

- **`AvailabilityManager` capacity zero and `IsFullyClosed` are treated identically.** A slot with `Capacity = 0` and one with an explicit closed flag both mark the slot as closed, masking the difference between "no capacity configured" and "deliberately closed."

---

## Missing Features

- **No endpoint to update employee role.** You can create users, toggle active status, and reset passwords, but cannot change someone's role without direct database access.

- **No client deactivation endpoint.** `IsActive` exists on the `Client` entity and `ClientsDelete` permission is seeded, but `ClientController` has no deactivation endpoint. The permission key exists but nothing uses it at the client level.

- **Employee fields are effectively read-only after creation.** `HourlyRate`, `MaxJobsPerDay`, and `EmployeeCode` are set in seed data but have no API endpoint to update them.

- **No payment deletion or correction endpoint.** Intentional presumably, but means any corrections must be made directly in the database, bypassing all status update logic.

- **No `PortalAuthController` integration tests.** The magic link token validation logic is only tested through Playwright E2E tests. A bug in that logic would not be caught by the integration test suite.

- **No unit test for `ReportingController.BuildInvoiceWorkbook`.** It is `internal static` and pure logic that could easily be tested with mock data.

---

## Data Model Issues

- **`BookingView` mapper silently drops service type.** `BookingMapper.ToResponse(BookingView view, int clientId)` sets `ServiceType = string.Empty` unconditionally despite `BookingView` having a populated `ServiceType` field. The paginated bookings list always returns empty string for service type.

- **`BookingView` mapper also silently drops vehicle and boat fields.** `BookingView` populates `LicensePlate`, `CarModel`, `BoatType`, and `LengthMeters` from the SQL view but the mapper ignores all of them. They are fetched but never surfaced in the paginated list response.

- **`BookingService` has no timestamps.** Every other major entity has `CreatedAt` and `UpdatedAt`. `BookingService` has neither, meaning price changes via `UpdateServicePriceAsync` have no record of when they happened beyond bumping the parent booking's `UpdatedAt`.

- **`ContactResponse` and `SiteResponse` omit timestamps.** Neither exposes `CreatedAt` or `UpdatedAt` despite the underlying entities having them. `ClientResponse` includes `CreatedAt`, making audit information inconsistently available.

- **`vw_EmployeeUtilization` mixes time windows.** `JobsAssigned` has no date filter but `JobsCompleted` only counts the last 30 days. The utilization numbers compare total assignments ever against recent completions.

- **List and detail endpoints return inconsistent data shapes.** `GetAllBookingsAsync` uses `BookingViews` while `GetBookingDetailByIdAsync` uses the full entity with includes. The same booking can show different fields and formats depending on whether it comes from the list or detail endpoint.

- **`DateTime` vs `DateOnly` inconsistency.** `Booking.ScheduledDate` uses `DateTime` in the entity but `DATE` in SQL, requiring `.Date` stripping throughout. `DateOverride.Date` uses `DateOnly` consistently. This inconsistency makes date comparisons slightly error-prone.

---

## Schema Issues

- **Two SQL schema files have diverged.** `cleaning_platform.sql` and `cleaning_platform_seed.sql` represent different schema states. The original adds `RecurringScheduleId` via `ALTER TABLE` while the seed file includes it in the original `CREATE TABLE`. The CI pipeline uses the seed file but the original is presumably the canonical schema. Running them produces different schemas.

- **Missing index in original schema.** `cleaning_platform.sql` creates the `IX_Bookings_RecurringScheduleId` index after a later `GO` block. If the script does not complete fully, the index is missing and `RecurringScheduleManager` queries that filter by `RecurringScheduleId` cause full table scans.

---

## Validation Gaps

- **`SopManager` does not validate `ServiceType`.** Invalid values fall through to a SQL constraint violation and a generic error message rather than a clean validation failure. `ServiceCatalogManager` validates this explicitly — `SopManager` should match.

- **`RecurringScheduleManager` does not validate `DayOfWeek` and `DayOfMonth` values.** Out-of-range values throw SQL exceptions rather than returning clean validation messages. Also `DayOfMonth` is capped at 28 in the constraint but this reasoning is undocumented.

- **`CreateAdminBookingRequest` accepts a typed enum for `ServiceType` on input but returns a string on output.** The frontend must know both enum values for sending and string representations for receiving.

---

## Test Issues

- **`AuthManagerTests.RegisterAsync_DuplicateUsername_ReturnsFail` has wrong input.** Registers `"First User"` and `"Second User"` which generate different usernames (`fuser` and `suser`). There is never a collision so the second registration succeeds and the test assertion fails. Should register the same person twice.

- **`EmployeeManagerTests.ToggleActiveAsync_SwitchesStatus` is fragile.** If only one user exists in the database, it uses the same user as both actor and target, which the manager explicitly rejects with "You cannot deactivate your own account."

- **`BookingManagerTests.UpdateStatusAsync_InvalidTransition_ReturnsFail` is permanently skipped.** Documents a known gap in business logic that should be addressed rather than permanently skipped.

- **Several integration tests have silent vacuous passes.** Patterns like `if (all.Items.Count == 0) return;` mean tests pass on an empty database without testing anything. Should either assert the precondition exists or be marked with `Skip`.

- **`TransactionScope` rollback interacts with multi-save managers.** Managers like `RecurringScheduleManager` call `SaveChangesAsync` multiple times. Partial state within a test can cause false passes or confusing failures on incomplete data rather than clear signals.

- **Playwright `test.skip` with visibility conditions.** Some tests skip when UI elements are not visible due to no available slots or no data, silently passing green rather than failing or being meaningfully skipped. Makes the suite look healthier than it is.

- **Booking flow Playwright tests are slow and fragile.** Each test re-navigates through steps 1 and 2 to test step 3. A failure in the availability API cascades into failures across many unrelated tests with confusing error messages.

- **`generate invoice from booking` test almost never runs.** Skips if the booking is not `Completed`, but the first booking in the list is unpredictable. Needs a dedicated completed booking fixture.

---

## Minor Issues

- **`EmailService` silently drops emails if SendGrid key is misconfigured.** Falls back to console logging with no error thrown and no user notification. No retry logic for transient failures.

- **`RecurringJobService` misses daily run on restart.** If the process restarts after the configured `RunHour`, the job does not run until the following day.

- **`Program.cs` fallback serves HTML for mistyped API routes.** `MapFallbackToFile("public/index.html")` returns `200 OK` with HTML for any unmatched route including typos in API paths. Automated monitoring checking status codes would miss broken API routes entirely. Should only apply to non-API routes.

- **`AppDbContext` global `datetime2` configuration applies unnecessarily to view entities.** Setting column types on keyless view entities does nothing harmful but adds noise to the model configuration.

- **`ClaimsPrincipalExtensions.GetEmployeeId` fallback to `Sub` is never needed.** `TokenManager.CreateAdminToken` sets both `NameIdentifier` and `Sub` to the same value, making the fallback dead code that hints at historical token design inconsistency.

- **`ScheduleMapper.ToAvailabilityResponse` closed day check is dead code for Sunday.** The loop never executes when `StartHour == EndHour == 0`, so the `defaultCapacity == 0` closed check is never reached for the Sunday seed pattern.

- **`KanbanManager.GetResourceGridAsync` month view loads all bookings into memory.** For a busy month the resource grid builds one column per employee, each scanning the full booking list. Works but does not scale gracefully.

- **`RoleMapper.ToPermissionResponse` uses an anonymous tuple instead of a named type.** Works but is less refactor-safe than a dedicated type.

- **Hardcoded seed password `ChangeMe123!` has no enforcement mechanism.** The name implies it should be changed but nothing reminds or requires anyone to actually change it after setup.

- **`KanbanManager.AsSplitQuery` does not fetch data atomically.** Multiple SQL queries mean data modified between queries could produce inconsistent board state. Acceptable tradeoff for a read-heavy dashboard but worth knowing.

- **`PortalDataController.GetClientId` returns `0` on failure, producing `404` instead of `401`.** A valid token missing the `client_id` claim gets a misleading not-found response instead of unauthorized.

- **`ReportingController.BuildInvoiceWorkbook` uses hardcoded column indices.** Adding or reordering headers without updating the corresponding cell writes silently misaligns columns.

- **`BookingMapper` input accepts typed enum, output returns string.** Inconsistent API contract for service type between request and response.

- **Scalar API documentation only available in development.** No way to explore the API on staging without either enabling it there or running locally. Worth a comment explaining the deliberate choice.

---

## What Is Genuinely Good

The naming is consistent and readable throughout. The separation between contracts, entities, and mappers is clean. The permission system is more granular than most internal tools bother with. The SQL schema is well-indexed for the actual query patterns used. The `PermissionKeys` static constructor that throws on sync failure is a good self-enforcing guard. The Playwright JWT pre-generation for portal auth avoids brittle email-flow testing. The `TransactionScope` rollback strategy for integration tests is clever. The global exception handler covers SQL error codes meaningfully. The overall codebase reads like a mature internal platform built by someone who thought carefully about the domain.
