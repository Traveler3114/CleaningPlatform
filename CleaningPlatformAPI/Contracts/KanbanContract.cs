namespace CleaningPlatformAPI.Contracts;

// ── Existing contracts (kept for employee view and API compat) ─────────────
public record KanbanBoardResponse(DateTime Date, List<KanbanColumnResponse> Columns, List<KanbanEmployeeResponse> Employees);
public record KanbanColumnResponse(string Status, List<KanbanCardResponse> Cards);
public record KanbanCardResponse(int BookingId, string ClientName, string? ClientPhone, string? SiteName, string? SiteAddress, int Hour, string ServiceType, int ServicesCount, List<AssignedEmployeeResponse> AssignedEmployees, bool HasSop, decimal SopCompletionPct);
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
    List<WeeklyBookingCard> Bookings
);

public record WeeklyBookingCard(
    int BookingId,
    int Hour,
    string ClientName,
    string? ClientPhone,
    string? SiteName,
    string? SiteAddress,
    string ServiceType,
    string Status,
    int ServicesCount,
    List<AssignedEmployeeResponse> AssignedEmployees,
    bool HasSop,
    decimal SopCompletionPct
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