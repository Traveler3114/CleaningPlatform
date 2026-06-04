using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Managers;

public class KanbanManager
{
    private readonly AppDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private static readonly string[] Statuses = ["Pending", "InProgress", "Completed", "Cancelled"];

    public KanbanManager(AppDbContext db, IStringLocalizer<SharedResources> localizer) { _db = db; _localizer = localizer; }

    // ── Existing methods (kept for API compat and employee view) ───────────

    public async Task<KanbanBoardResponse> GetBoardAsync(DateTime date, int? employeeId = null, CancellationToken ct = default)
    {
        var query = BaseBookingQuery().Where(b => b.ScheduledDate.Date == date.Date);
        if (employeeId.HasValue)
            query = query.Where(b => b.Assignments.Any(a => a.EmployeeId == employeeId.Value));

        var bookings = await query.OrderBy(b => b.ScheduledTimeSlot).ToListAsync(ct);
        var completedCounts = await GetCompletedCountsAsync(bookings, ct);
        return new KanbanBoardResponse(date.Date, BuildColumns(bookings, completedCounts), await GetEmployeesForDateAsync(date.Date, ct));
    }

    public async Task<KanbanBoardResponse> GetPipelineAsync(CancellationToken ct = default)
    {
        var bookings = await BaseBookingQuery()
            .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.InProgress)
            .OrderBy(b => b.ScheduledDate).ThenBy(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);
        var completedCounts = await GetCompletedCountsAsync(bookings, ct);
        return new KanbanBoardResponse(DateTime.UtcNow.Date, BuildColumns(bookings, completedCounts), await GetEmployeesForDateAsync(DateTime.UtcNow.Date, ct));
    }

    public async Task<WeeklyBoardResponse> GetWeekAsync(DateTime weekStart, int? filterEmployeeId = null, CancellationToken ct = default)
    {
        var weekEnd = weekStart.AddDays(6);

        var query = BaseBookingQuery()
            .Where(b => b.ScheduledDate.Date >= weekStart.Date && b.ScheduledDate.Date <= weekEnd.Date);

        if (filterEmployeeId.HasValue)
            query = query.Where(b => b.Assignments.Any(a => a.EmployeeId == filterEmployeeId.Value));

        var bookings = await query.OrderBy(b => b.ScheduledDate).ThenBy(b => b.ScheduledTimeSlot).ToListAsync(ct);
        var completedCounts = await GetCompletedCountsAsync(bookings, ct);

        var today = DateTime.UtcNow.Date;
        var days = Enumerable.Range(0, 7).Select(i =>
        {
            var date = weekStart.AddDays(i);
            var dayBookings = bookings
                .Where(b => b.ScheduledDate.Date == date.Date)
                .Select(b => ToWeeklyCard(b, completedCounts.GetValueOrDefault(b.Id, 0)))
                .ToList();

            return new DayColumnResponse(
                date,
                date.ToString("ddd"),
                date.ToString("ddd d MMM"),
                date.Date == today,
                dayBookings
            );
        }).ToList();

        var employees = await GetEmployeesForWeekAsync(weekStart, weekEnd, ct);
        return new WeeklyBoardResponse(weekStart, weekEnd, days, employees);
    }

    public async Task<WeeklyBoardResponse> GetEmployeeWeekAsync(int employeeId, DateTime weekStart, CancellationToken ct = default)
    {
        var weekEnd = weekStart.AddDays(6);

        var bookings = await BaseBookingQuery()
            .Where(b => b.ScheduledDate.Date >= weekStart.Date
                     && b.ScheduledDate.Date <= weekEnd.Date
                     && b.Assignments.Any(a => a.EmployeeId == employeeId))
            .OrderBy(b => b.ScheduledDate).ThenBy(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);

        var completedCounts = await GetCompletedCountsAsync(bookings, ct);

        var today = DateTime.UtcNow.Date;
        var days = Enumerable.Range(0, 7).Select(i =>
        {
            var date = weekStart.AddDays(i);
            var dayBookings = bookings
                .Where(b => b.ScheduledDate.Date == date.Date)
                .Select(b => ToWeeklyCard(b, completedCounts.GetValueOrDefault(b.Id, 0)))
                .ToList();

            return new DayColumnResponse(
                date,
                date.ToString("ddd"),
                date.ToString("ddd d MMM"),
                date.Date == today,
                dayBookings
            );
        }).ToList();

        return new WeeklyBoardResponse(weekStart, weekEnd, days, []);
    }

    // ── New resource grid method (admin calendar) ──────────────────────────

    public async Task<ResourceGridResponse> GetResourceGridAsync(
        DateTime anchorDate,
        string view,
        CancellationToken ct = default)
    {
        // Determine date range
        DateTime rangeStart;
        DateTime rangeEnd;

        switch (view.ToLower())
        {
            case "day":
                rangeStart = anchorDate.Date;
                rangeEnd = anchorDate.Date;
                break;
            case "month":
                rangeStart = new DateTime(anchorDate.Year, anchorDate.Month, 1);
                rangeEnd = rangeStart.AddMonths(1).AddDays(-1);
                break;
            default: // week
                rangeStart = GetMondayOf(anchorDate);
                rangeEnd = rangeStart.AddDays(6);
                break;
        }

        // Load bookings in range (exclude cancelled)
        var bookings = await BaseBookingQuery()
            .Where(b => b.ScheduledDate.Date >= rangeStart.Date
                     && b.ScheduledDate.Date <= rangeEnd.Date
                     && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.ScheduledDate)
            .ThenBy(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);

        // Load all active employees
        var employees = await _db.Employees
            .Include(e => e.Role)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .ToListAsync(ct);

        // Unassigned = bookings with no assignments at all
        var unassigned = bookings
            .Where(b => !b.Assignments.Any())
            .Select(ToResourceCard)
            .ToList();

        // Build employee columns
        var employeeColumns = employees.Select(e =>
        {
            var empBookings = bookings
                .Where(b => b.Assignments.Any(a => a.EmployeeId == e.Id))
                .Select(ToResourceCard)
                .ToList();

            return new ResourceEmployeeColumn(
                e.Id,
                $"{e.FirstName} {e.LastName}".Trim(),
                e.Role?.Name ?? string.Empty,
                !empBookings.Any(),
                empBookings
            );
        }).ToList();

        return new ResourceGridResponse(anchorDate, view, employeeColumns, unassigned);
    }

    public async Task<List<EquipmentWarningResponse>> GetEquipmentWarningsAsync(DateTime date, CancellationToken ct = default)
    {
        var bookings = await _db.Bookings
            .Include(b => b.BookingServices).ThenInclude(bs => bs.ServiceCatalog)
                .ThenInclude(sc => sc.InventoryRequirements).ThenInclude(r => r.Inventory)
            .Where(b => b.ScheduledDate.Date == date.Date && b.Status != BookingStatus.Cancelled)
            .ToListAsync(ct);

        var equipment = await _db.Inventory.Where(i => i.Type == "Equipment").ToListAsync(ct);
        var warnings = new List<EquipmentWarningResponse>();

        foreach (var item in equipment)
        {
            var hourGroups = bookings
                .SelectMany(b => b.BookingServices
                    .Where(bs => bs.ServiceCatalog.InventoryRequirements.Any(r => r.InventoryId == item.Id))
                    .Select(bs => new
                    {
                        Hour = b.ScheduledTimeSlot?.Hours ?? 0,
                        Requirement = bs.ServiceCatalog.InventoryRequirements.First(r => r.InventoryId == item.Id)
                    }))
                .GroupBy(x => x.Hour);

            foreach (var group in hourGroups)
            {
                var required = group.Sum(x => x.Requirement.QuantityNeeded);
                if (required > item.Quantity)
                {
                    warnings.Add(new EquipmentWarningResponse
                    {
                        InventoryName = item.Name,
                        Unit = item.Unit.ToString(),
                        Hour = group.Key,
                        Available = item.Quantity,
                        Required = required
                    });
                }
            }
        }

        return warnings;
    }

    // ── Shared helpers ─────────────────────────────────────────────────────

    private IQueryable<Entities.Booking> BaseBookingQuery() => _db.Bookings
        .Include(b => b.Client).ThenInclude(c => c.Contacts)
        .Include(b => b.Site)
        .Include(b => b.BookingServices)
        .Include(b => b.SopAssignments).ThenInclude(sa => sa.SopTemplate).ThenInclude(st => st.ChecklistItems)
        .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
        .AsSplitQuery();

    private List<KanbanColumnResponse> BuildColumns(List<Entities.Booking> bookings, Dictionary<int, int> completedCounts) => Statuses
        .Select(status => new KanbanColumnResponse(
            status,
            bookings.Where(b => b.Status.ToString() == status).Select(b => ToCard(b, completedCounts.GetValueOrDefault(b.Id, 0))).ToList()))
        .ToList();

    private static KanbanCardResponse ToCard(Entities.Booking b, int completedItems = 0)
    {
        var primaryContact = b.Client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)
            ?? b.Client.Contacts.FirstOrDefault(c => c.IsActive);
        var pct = GetSopProgress(b, completedItems);
        return new KanbanCardResponse(
            b.Id, b.Client.ClientName, primaryContact?.Phone,
            b.Site?.SiteName, b.Site?.Address,
            b.ScheduledDate, b.ScheduledTimeSlot?.Hours ?? 0,
            b.ServiceType.ToString(), b.Status.ToString(),
            b.BookingServices.Count,
            MapAssignees(b), b.SopAssignments.Count != 0, pct);
    }

    private static KanbanCardResponse ToWeeklyCard(Entities.Booking b, int completedItems = 0)
    {
        var primaryContact = b.Client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive)
            ?? b.Client.Contacts.FirstOrDefault(c => c.IsActive);
        var pct = GetSopProgress(b, completedItems);
        return new KanbanCardResponse(
            b.Id,
            b.Client.ClientName,
            primaryContact?.Phone,
            b.Site?.SiteName,
            b.Site?.Address,
            b.ScheduledDate,
            b.ScheduledTimeSlot?.Hours ?? 0,
            b.ServiceType.ToString(),
            b.Status.ToString(),
            b.BookingServices.Count,
            MapAssignees(b),
            b.SopAssignments.Count != 0,
            pct);
    }

    private static ResourceBookingCard ToResourceCard(Entities.Booking b) => new(
        b.Id,
        b.ClientId,
        b.Client.ClientName,
        b.ServiceType.ToString(),
        b.ScheduledDate,
        b.ScheduledTimeSlot?.Hours ?? 0,
        b.Status.ToString(),
        b.Site?.SiteName
    );

    private static List<AssignedEmployeeResponse> MapAssignees(Entities.Booking b) =>
        b.Assignments.Select(a => new AssignedEmployeeResponse
        {
            EmployeeId = a.EmployeeId,
            FullName = $"{a.Employee.FirstName} {a.Employee.LastName}".Trim(),
            Role = a.Employee.Role?.Name ?? string.Empty
        }).ToList();

    private static decimal GetSopProgress(Entities.Booking b, int completedItems)
    {
        var totalItems = b.SopAssignments.Sum(sa => sa.SopTemplate.ChecklistItems.Count);
        return totalItems == 0 ? 0 : Math.Round((decimal)completedItems / totalItems * 100, 1);
    }

    private async Task<Dictionary<int, int>> GetCompletedCountsAsync(List<Entities.Booking> bookings, CancellationToken ct)
    {
        var bookingIds = bookings.Select(b => b.Id).ToList();
        if (bookingIds.Count == 0) return new();
        return await _db.ChecklistResponses
            .Where(r => bookingIds.Contains(r.BookingId) && r.IsCompleted)
            .GroupBy(r => r.BookingId)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
    }

    private async Task<List<KanbanEmployeeResponse>> GetEmployeesForDateAsync(DateTime date, CancellationToken ct)
    {
        var employees = await _db.Employees.Include(e => e.Role).Where(e => e.IsActive)
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(ct);

        var counts = await _db.BookingAssignments
            .Where(a => a.Booking.ScheduledDate.Date == date.Date && a.Booking.Status != BookingStatus.Cancelled)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, ct);

        return employees.Select(e =>
        {
            counts.TryGetValue(e.Id, out var count);
            return new KanbanEmployeeResponse(
                e.Id, $"{e.FirstName} {e.LastName}".Trim(),
                e.Role?.Name ?? string.Empty, count,
                !e.MaxJobsPerDay.HasValue || count < e.MaxJobsPerDay.Value);
        }).ToList();
    }

    private async Task<List<KanbanEmployeeResponse>> GetEmployeesForWeekAsync(DateTime weekStart, DateTime weekEnd, CancellationToken ct)
    {
        var employees = await _db.Employees.Include(e => e.Role).Where(e => e.IsActive)
            .OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(ct);

        var counts = await _db.BookingAssignments
            .Where(a => a.Booking.ScheduledDate.Date >= weekStart.Date
                     && a.Booking.ScheduledDate.Date <= weekEnd.Date
                     && a.Booking.Status != BookingStatus.Cancelled)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Count, ct);

        return employees.Select(e =>
        {
            counts.TryGetValue(e.Id, out var count);
            return new KanbanEmployeeResponse(
                e.Id, $"{e.FirstName} {e.LastName}".Trim(),
                e.Role?.Name ?? string.Empty, count,
                !e.MaxJobsPerDay.HasValue || count < e.MaxJobsPerDay.Value);
        }).ToList();
    }

    private static DateTime GetMondayOf(DateTime date)
    {
        var d = date.Date;
        var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
        return d.AddDays(-diff);
    }
}