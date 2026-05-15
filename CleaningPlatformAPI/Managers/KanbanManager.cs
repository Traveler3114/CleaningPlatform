using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Enums;

namespace CleaningPlatformAPI.Managers;

public class KanbanManager
{
    private readonly AppDbContext _db;
    public KanbanManager(AppDbContext db) => _db = db;

    public async Task<WeeklyBoardResponse> GetWeekAsync(DateTime weekStart, int? employeeId, CancellationToken ct = default)
    {
        var start = weekStart.Date;
        var end = start.AddDays(6);
        var query = BaseBookingQuery().Where(b => b.ScheduledDate.Date >= start && b.ScheduledDate.Date <= end);
        if (employeeId.HasValue) query = query.Where(b => b.Assignments.Any(a => a.EmployeeId == employeeId.Value));
        var bookings = await query.OrderBy(b => b.ScheduledDate).ThenBy(b => b.ScheduledTimeSlot).ToListAsync(ct);
        var days = Enumerable.Range(0, 7).Select(i => start.AddDays(i)).Select(d => new DayColumnResponse(d, d.ToString("ddd"), bookings.Where(b => b.ScheduledDate.Date == d.Date).Select(ToWeeklyCard).ToList())).ToList();
        var employees = await GetEmployeesAsync(start, end, ct);
        return new WeeklyBoardResponse(start, days, employees);
    }

    public Task<WeeklyBoardResponse> GetEmployeeWeekAsync(int employeeId, DateTime weekStart, CancellationToken ct = default) =>
        GetWeekAsync(weekStart, employeeId, ct);

    private IQueryable<Entities.Booking> BaseBookingQuery() => _db.Bookings
        .Include(b => b.Client).ThenInclude(c => c.Contacts)
        .Include(b => b.Site)
        .Include(b => b.BookingServices)
        .Include(b => b.SopAssignments).ThenInclude(sa => sa.SopTemplate).ThenInclude(st => st.ChecklistItems)
        .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
        .Include(b => b.Assignments).ThenInclude(a => a.ChecklistResponses)
        .AsSplitQuery();

    private static WeeklyBookingCard ToWeeklyCard(Entities.Booking b)
    {
        var primaryContact = b.Client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive) ?? b.Client.Contacts.FirstOrDefault(c => c.IsActive);
        var totalItems = b.SopAssignments.Sum(sa => sa.SopTemplate.ChecklistItems.Count);
        var completed = b.Assignments.SelectMany(a => a.ChecklistResponses).Count(r => r.IsCompleted);
        var pct = totalItems == 0 ? 0 : Math.Round((decimal)completed / totalItems * 100, 1);
        return new WeeklyBookingCard(b.Id, b.ScheduledTimeSlot?.Hours ?? 0, b.Client.ClientName, primaryContact?.Phone, b.Site?.SiteName, b.ServiceType.ToString(), b.Status.ToString(), b.BookingServices.Count,
            b.Assignments.Select(a => new AssignedEmployeeResponse { AssignmentId = a.Id, EmployeeId = a.EmployeeId, FullName = $"{a.Employee.FirstName} {a.Employee.LastName}".Trim(), Role = a.Employee.Role?.Name ?? string.Empty }).ToList(),
            b.SopAssignments.Count != 0, pct);
    }

    private async Task<List<KanbanEmployeeResponse>> GetEmployeesAsync(DateTime start, DateTime end, CancellationToken ct)
    {
        var employees = await _db.Employees.Include(e => e.Role).Where(e => e.IsActive).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(ct);
        var counts = await _db.BookingAssignments.Where(a => a.Booking.ScheduledDate.Date >= start && a.Booking.ScheduledDate.Date <= end && a.Booking.Status != BookingStatus.Cancelled)
            .GroupBy(a => a.EmployeeId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        return employees.Select(e => { counts.TryGetValue(e.Id, out var c); return new KanbanEmployeeResponse(e.Id, $"{e.FirstName} {e.LastName}".Trim(), e.Role?.Name ?? string.Empty, c, !e.MaxJobsPerDay.HasValue || c < e.MaxJobsPerDay.Value); }).ToList();
    }
}
