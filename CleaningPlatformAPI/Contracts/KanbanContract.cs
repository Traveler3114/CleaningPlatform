namespace CleaningPlatformAPI.Contracts;

public record KanbanBoardResponse(DateTime Date, List<KanbanColumnResponse> Columns, List<KanbanEmployeeResponse> Employees);
public record KanbanColumnResponse(string Status, List<KanbanCardResponse> Cards);
public record KanbanCardResponse(int BookingId, string ClientName, string? ClientPhone, string? SiteName, string? SiteAddress, int Hour, string ServiceType, int ServicesCount, List<AssignedEmployeeResponse> AssignedEmployees, bool HasSop, decimal SopCompletionPct);
public record KanbanEmployeeResponse(int EmployeeId, string FullName, string Role, int JobsToday, bool IsFree);
