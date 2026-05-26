# Agent Fix Plan — Cleaning Operations Platform

This document defines exactly what the agent must fix, how to fix it, and what must not be touched.
Work through each task in order. Do not combine tasks. After each task, the code must still compile and existing tests must still pass.

---

## Rules before starting

- Read PROJECT.md fully before writing any code
- One task at a time — do not batch multiple fixes into one change
- Do not touch anything listed in the Off-Limits section at the bottom
- If a fix requires a schema change, stop and flag it for human review before proceeding
- All new code must follow the conventions in PROJECT.md exactly

---

## Task 1 — Fix EnsureServiceSopsAssignedAsync duplicate insertion risk

**File:** `CleaningPlatformAPI/Managers/SopManager.cs`

**Problem:** `EnsureServiceSopsAssignedAsync` inserts into `BookingSopAssignments` directly without checking if the assignment already exists. If called twice for the same booking it throws a SQL unique constraint exception instead of handling gracefully.

**Fix:** Before inserting each template assignment, check if it already exists using `AnyAsync`. Only insert if it does not. This is the same pattern already used in `AssignSopToBookingAsync`.

```csharp
var alreadyAssignedIds = await _db.BookingSopAssignments
    .Where(a => a.BookingId == bookingId)
    .Select(a => a.SopTemplateId)
    .ToListAsync(ct);

var toAssign = linkedTemplateIds.Except(alreadyAssignedIds).ToList();
```

**Verify:** Call the method twice for the same booking in a test — it must not throw.

---

## Task 2 — Add ServiceType validation to SopManager

**File:** `CleaningPlatformAPI/Managers/SopManager.cs`

**Problem:** `CreateTemplateAsync` and `UpdateTemplateAsync` accept any string for `ServiceType` and let the SQL constraint catch invalid values, returning a generic error. `ServiceCatalogManager` validates the same concept explicitly.

**Fix:** Add explicit validation before saving in both `CreateTemplateAsync` and `UpdateTemplateAsync`:

```csharp
var validServiceTypes = new[] { "Vehicle", "SiteBased", "Boat", "Generic" };
if (!validServiceTypes.Contains(dto.ServiceType?.Trim()))
    return OperationResult<SopTemplateResponse>.Fail("ServiceType must be one of: Vehicle, SiteBased, Boat, Generic.");
```

**Verify:** Calling create or update with an invalid ServiceType must return `Success = false` with a clear message, not a 500.
