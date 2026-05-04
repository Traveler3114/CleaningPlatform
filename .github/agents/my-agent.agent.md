---
# Fill in the fields below to create a basic custom agent for your repository.
# The Copilot CLI can be used for local testing: https://gh.io/customagents/cli
# To make this agent available, merge this file into the default repository branch.
# For format details, see: https://gh.io/customagents/config
name: Vehicle Cleaning Agent
description: A GitHub Copilot agent for rapid development, understanding, and troubleshooting of the Vehicle Cleaning booking system, including API, admin dashboard, and customer flows.
---

# My Agent

# Vehicle Cleaning System — Agent Build Instructions

## Project Structure
```
VehicleCleaning/
├── VehicleCleaningAPI/          ← ASP.NET Core Web API + Razor Pages (Admin)
│   ├── Controllers/
│   │   ├── AvailabilityController.cs
│   │   ├── BookingController.cs
│   │   ├── ScheduleController.cs
│   │   └── OverrideController.cs
│   ├── Pages/                   ← Razor Pages Admin Dashboard
│   │   ├── Index.cshtml          ← Daily view (home)
│   │   ├── Bookings.cshtml
│   │   ├── Schedule.cshtml
│   │   └── Shared/
│   │       └── _Layout.cshtml
│   ├── Pages/Index.cshtml.cs
│   ├── Pages/Bookings.cshtml.cs
│   ├── Pages/Schedule.cshtml.cs
│   ├── wwwroot/
│   │   └── css/style.css         ← Shared admin styles
│   ├── Entities/
│   │   ├── WeeklySchedule.cs
│   │   ├── SlotOverride.cs
│   │   └── Booking.cs
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Managers/
│   │   ├── AvailabilityManager.cs
│   │   ├── BookingManager.cs
│   │   ├── ScheduleManager.cs
│   │   └── OverrideManager.cs
│   ├── Dtos/
│   │   ├── AvailabilityDto.cs
│   │   ├── BookingDto.cs
│   │   ├── CreateBookingDto.cs
│   │   ├── WeeklyScheduleDto.cs
│   │   └── SlotOverrideDto.cs
│   ├── Common/
│   │   └── OperationResult.cs
│   └── Program.cs
├── CustomerWebsite/             ← Plain HTML/CSS/JS
│   ├── index.html
│   ├── css/style.css
│   └── js/app.js
```

---

## API — No Authentication
- No ASP.NET Identity, JWT, or authentication middleware.
- All endpoints are public.

---

## Admin Dashboard (Razor Pages)
- Implemented inside **VehicleCleaningAPI** using Razor Pages.
- Razor Pages **call managers directly** (no HTTP calls to own API).
- Shared layout + shared CSS in wwwroot/css/style.css.
- Navigation links between all three pages.

### Pages

- **Index.cshtml** (Daily View)
  - Date picker (defaults today).
  - On change:
    - Use AvailabilityManager for slots
    - Use BookingManager for bookings
  - Table: time, capacity, booked, available, override (inline form calls OverrideManager.CreateOverrideAsync).

- **Bookings.cshtml** (Bookings List)
  - Date picker, table: name, phone, hour, status.
  - Status dropdown uses BookingManager.UpdateStatusAsync.

- **Schedule.cshtml** (Weekly Schedule)
  - Table with 7 rows (days).
  - Editable: closed, start, end, capacity.
  - Save uses ScheduleManager.UpdateDayAsync.

---

## Database Models

### WeeklySchedule
```csharp
public class WeeklySchedule
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }      // 0=Sun, 1=Mon ... 6=Sat
    public bool IsClosed { get; set; }
    public int StartHour { get; set; }      // e.g. 8
    public int EndHour { get; set; }        // e.g. 17
    public int DefaultCapacity { get; set; } // e.g. 2
}
```

### SlotOverride
```csharp
public class SlotOverride
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int? Hour { get; set; }          // null = whole day override
    public bool IsClosed { get; set; }
    public int? Capacity { get; set; }      // null = use default
}
```

### Booking
```csharp
public class Booking
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string Phone { get; set; }
    public DateTime Date { get; set; }
    public int Hour { get; set; }
    public BookingStatus Status { get; set; } // Reserved, Cancelled, Completed
    public DateTime CreatedAt { get; set; }
}

public enum BookingStatus { Reserved, Cancelled, Completed }
```

### AppDbContext
- Seed WeeklySchedule with 7 rows on first run.
- Store enums as strings (use EnumToStringConverter).
- Enable CORS for all origins (prototype).

---

## Managers Pattern
Controllers are thin and delegate to managers. All logic resides in manager classes.

### AvailabilityManager
- For a given date:
  1. Get WeeklySchedule for that day of week.
  2. Check for SlotOverride where Date = date and Hour = null → day-level override.
  3. For each hour, check SlotOverride where Date = date and Hour = h → hour-level override.
  4. Count Booking records for that date + hour where Status != Cancelled.
  5. Return slots: hour, capacity, booked count, available count, isClosed.
- **Priority chain:** HourOverride → DayOverride → WeeklySchedule.

### BookingManager
- GetBookingsAsync(date) — all bookings for a date.
- GetAllBookingsAsync() — admin: all bookings.
- CreateBookingAsync(dto) — validate slot is open/capacity OK, then create record.
- UpdateStatusAsync(id, status) — admin updates booking status.

### ScheduleManager
- GetScheduleAsync() — all 7 WeeklySchedule rows.
- UpdateDayAsync(dayOfWeek, dto) — update a single day's schedule.

### OverrideManager
- GetOverridesAsync() — all future overrides.
- CreateOverrideAsync(dto) — create override.
- DeleteOverrideAsync(id) — delete override.
- **Nightly cleanup:** delete overrides where Date < today - 30 days.

---

## API Endpoints

| Method | Endpoint | Action |
| ------ | ----------------------------------- | ------------------------------------------------ |
| GET | /api/availability?date=YYYY-MM-DD | List slots for that date |
| GET | /api/bookings?date=YYYY-MM-DD | Bookings for a date (admin) |
| POST | /api/bookings | Create booking { name, phone, date, hour } |
| PUT | /api/bookings/{id}/status | Update status { status } |
| GET | /api/schedule | All 7 days’ schedules |
| PUT | /api/schedule/{dayOfWeek} | Update a day |
| GET | /api/overrides | All overrides |
| POST | /api/overrides | Create override |
| DELETE | /api/overrides/{id} | Delete override |

- **All responses:** wrapped in OperationResult<T>.

---

## OperationResult
Copy the OperationResult<T> pattern from GetThereAPI:
```csharp
public class OperationResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public static OperationResult<T> Ok(T data) { ... }
    public static OperationResult<T> Fail(string message) { ... }
}
```

---

## Customer Website
- Single index.html. Pure JS (no framework).
- **4-step flow:** Date → Time → Details → Confirmation.
  1. Step 1: Calendar UI (no API call).
  2. Step 2: On date selected, calls GET /api/availability?date= and renders slots.
  3. Step 3: Collect name and phone.
  4. Step 4: Call POST /api/bookings, show confirmation.
- Visual: Use vehicle_cleaning_booking.html as prototype. Remove hardcoded logic, use real API.

---

## Program.cs Notes
- No Identity, no JWT, no auth.
- Register all managers with AddScoped.
- Register Razor Pages with AddRazorPages().
- Map Razor Pages with app.MapRazorPages().
- Enable CORS: AllowAnyOrigin, AllowAnyMethod, AllowAnyHeader.
- Use SQL Server with EF Core.
- Call db.Database.EnsureCreated() on startup.
- Seed WeeklySchedule if empty:
  - Mon–Fri 08–17
  - Sat 09–13
  - Sun closed
  - All capacity 2

---

# My Agent
This agent provides expertise and automation for the Vehicle Cleaning booking system repository.  
It understands the API, admin dashboard, database models, and customer website flow to answer questions, help generate tests and endpoints, and assist developers in all parts of the stack.
