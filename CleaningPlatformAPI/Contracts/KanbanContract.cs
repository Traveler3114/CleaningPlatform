namespace CleaningPlatformAPI.Contracts;

public record WeeklyBoardResponse(DateTime WeekStart, List<DayColumnResponse> Days, List<KanbanEmployeeResponse> Employees);
public record DayColumnResponse(DateTime Date, string DayName, List<WeeklyBookingCard> Bookings);
public record WeeklyBookingCard(int BookingId, int Hour, string ClientName, string? ClientPhone, string? SiteName, string ServiceType, string Status, int ServicesCount, List<AssignedEmployeeResponse> AssignedEmployees, bool HasSop, decimal SopCompletionPct);
public record KanbanEmployeeResponse(int EmployeeId, string FullName, string Role, int JobsToday, bool IsFree);
