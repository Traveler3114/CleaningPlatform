namespace CleaningPlatformAPI.Contracts;

// ── Existing contracts (kept for employee view and API compat) ─────────────
public record KanbanBoardResponse(DateTime Date, List<KanbanColumnResponse> Columns, List<KanbanEmployeeResponse> Employees);
public record KanbanColumnResponse(string Status, List<KanbanCardResponse> Cards);
public record KanbanCardResponse(int BookingId, string ClientName, string? ClientPhone, string? SiteName, string? SiteAddress, DateTime Date, int Hour, string ServiceType, string Status, int ServicesCount, List<AssignedEmployeeResponse> AssignedEmployees, bool HasSop, decimal SopCompletionPct);
public record KanbanEmployeeResponse(int EmployeeId, string FullName, string Role, int JobsToday, bool IsFree);

public record WeeklyBoardResponse(
    DateTime WeekStart,
    DateTime WeekEnd,
    List<DayColumnResponse> Days,
    List<KanbanEmployeeResponse> Employees
);

public record DayColumnResponse(
    DateTime Date,
    string DayName,
    string DayLabel,
    bool IsToday,
    List<KanbanCardResponse> Bookings
);

// ── New resource grid contracts (admin calendar only) ──────────────────────
public record ResourceGridResponse(
    DateTime AnchorDate,
    string View,
    List<ResourceEmployeeColumn> Employees,
    List<ResourceBookingCard> Unassigned
);

public record ResourceEmployeeColumn(
    int EmployeeId,
    string FullName,
    string Role,
    bool IsFree,
    List<ResourceBookingCard> Bookings
);

public record ResourceBookingCard(
    int BookingId,
    int ClientId,
    string ClientName,
    string ServiceType,
    DateTime Date,
    int Hour,
    string Status,
    string? SiteName
);

public class EquipmentWarningResponse
{
    public string InventoryName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Hour { get; set; }
    public decimal Available { get; set; }
    public decimal Required { get; set; }
}