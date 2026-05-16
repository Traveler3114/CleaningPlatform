namespace CleaningPlatformAPI.Contracts;

// ── Existing board (kept for pipeline API compat) ──────────────────────────
public record KanbanBoardResponse(DateTime Date, List<KanbanColumnResponse> Columns, List<KanbanEmployeeResponse> Employees);
public record KanbanColumnResponse(string Status, List<KanbanCardResponse> Cards);
public record KanbanCardResponse(int BookingId, string ClientName, string? ClientPhone, string? SiteName, string? SiteAddress, int Hour, string ServiceType, int ServicesCount, List<AssignedEmployeeResponse> AssignedEmployees, bool HasSop, decimal SopCompletionPct);
public record KanbanEmployeeResponse(int EmployeeId, string FullName, string Role, int JobsToday, bool IsFree);

// ── Weekly calendar board ──────────────────────────────────────────────────
public record WeeklyBoardResponse(
    DateTime WeekStart,
    DateTime WeekEnd,
    List<DayColumnResponse> Days,
    List<KanbanEmployeeResponse> Employees
);

public record DayColumnResponse(
    DateTime Date,
    string DayName,       // e.g. "Mon", "Tue"
    string DayLabel,      // e.g. "Mon 12 May"
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
