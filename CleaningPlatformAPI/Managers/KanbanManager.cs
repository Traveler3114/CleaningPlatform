using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Enums;

namespace CleaningPlatformAPI.Managers;

public class KanbanManager
{
    private readonly AppDbContext _db;
    private static readonly string[] Statuses = ["Pending", "Confirmed", "InProgress", "Completed", "Cancelled"];

    public KanbanManager(AppDbContext db) => _db = db;

    public async Task<KanbanBoardResponse> GetBoardAsync(DateTime date, int? employeeId = null, CancellationToken ct = default)
    {
        var query = BaseBookingQuery().Where(b => b.ScheduledDate.Date == date.Date);
        if (employeeId.HasValue)
            query = query.Where(b => b.Assignments.Any(a => a.EmployeeId == employeeId.Value));

        var bookings = await query.OrderBy(b => b.ScheduledTimeSlot).ToListAsync(ct);
        return new KanbanBoardResponse(date.Date, BuildColumns(bookings), await GetEmployeesAsync(date.Date, ct));
    }

    public async Task<KanbanBoardResponse> GetPipelineAsync(CancellationToken ct = default)
    {
        var bookings = await BaseBookingQuery()
            .Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.InProgress)
            .OrderBy(b => b.ScheduledDate)
            .ThenBy(b => b.ScheduledTimeSlot)
            .ToListAsync(ct);
        return new KanbanBoardResponse(DateTime.UtcNow.Date, BuildColumns(bookings), await GetEmployeesAsync(DateTime.UtcNow.Date, ct));
    }

    private IQueryable<Entities.Booking> BaseBookingQuery() => _db.Bookings
        .Include(b => b.Client).ThenInclude(c => c.Contacts)
        .Include(b => b.Site)
        .Include(b => b.BookingServices)
        .Include(b => b.SopAssignments).ThenInclude(sa => sa.SopTemplate).ThenInclude(st => st.ChecklistItems)
        .Include(b => b.Assignments).ThenInclude(a => a.Employee).ThenInclude(e => e.Role)
        .Include(b => b.Assignments).ThenInclude(a => a.ChecklistResponses)
        .AsSplitQuery();

    private List<KanbanColumnResponse> BuildColumns(List<Entities.Booking> bookings) => Statuses
        .Select(status => new KanbanColumnResponse(status, bookings.Where(b => b.Status.ToString() == status).Select(ToCard).ToList()))
        .ToList();

    private static KanbanCardResponse ToCard(Entities.Booking b)
    {
        var primaryContact = b.Client.Contacts.FirstOrDefault(c => c.IsPrimary && c.IsActive) ?? b.Client.Contacts.FirstOrDefault(c => c.IsActive);
        var totalItems = b.SopAssignments.Sum(sa => sa.SopTemplate.ChecklistItems.Count);
        var completed = b.Assignments.SelectMany(a => a.ChecklistResponses).Count(r => r.IsCompleted);
        var pct = totalItems == 0 ? 0 : Math.Round((decimal)completed / totalItems * 100, 1);
        return new KanbanCardResponse(
            b.Id,
            b.Client.ClientName,
            primaryContact?.Phone,
            b.Site?.SiteName,
            b.Site?.Address,
            b.ScheduledTimeSlot?.Hours ?? 0,
            b.ServiceType.ToString(),
            b.BookingServices.Count,
            b.Assignments.Select(a => new AssignedEmployeeResponse { AssignmentId = a.Id, EmployeeId = a.EmployeeId, FullName = $"{a.Employee.FirstName} {a.Employee.LastName}".Trim(), Role = a.Employee.Role?.Name ?? string.Empty }).ToList(),
            b.SopAssignments.Count != 0,
            pct);
    }

    private async Task<List<KanbanEmployeeResponse>> GetEmployeesAsync(DateTime date, CancellationToken ct)
    {
        var employees = await _db.Employees.Include(e => e.Role).Where(e => e.IsActive).OrderBy(e => e.FirstName).ThenBy(e => e.LastName).ToListAsync(ct);
        var counts = await _db.BookingAssignments.Where(a => a.Booking.ScheduledDate.Date == date.Date && a.Booking.Status != BookingStatus.Cancelled).GroupBy(a => a.EmployeeId).Select(g => new { EmployeeId = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.EmployeeId, x => x.Count, ct);
        return employees.Select(e =>
        {
            counts.TryGetValue(e.Id, out var count);
            return new KanbanEmployeeResponse(e.Id, $"{e.FirstName} {e.LastName}".Trim(), e.Role?.Name ?? string.Empty, count, !e.MaxJobsPerDay.HasValue || count < e.MaxJobsPerDay.Value);
        }).ToList();
    }
}
