using CleaningPlatformAPI.Contracts;

namespace CleaningPlatformAPI.Contracts;

public class RecurringScheduleResponse
{
    public int Id { get; set; }
    public int SourceBookingId { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int AutoGenerateWeeksAhead { get; set; }
    public bool IsActive { get; set; }
    public DateOnly? EndsOn { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UpcomingCount { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? SiteName { get; set; }
}

public record CreateRecurringScheduleRequest
{
    public string Frequency { get; set; } = string.Empty;
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int AutoGenerateWeeksAhead { get; set; } = 4;
    public DateOnly? EndsOn { get; set; }
}

public record UpdateRecurringScheduleRequest
{
    public string Frequency { get; set; } = string.Empty;
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public int AutoGenerateWeeksAhead { get; set; } = 4;
    public DateOnly? EndsOn { get; set; }
}

public record EndSeriesRequest
{
    public DateOnly EndsOn { get; set; }
}

public class GenerateResult
{
    public List<BookingResponse> Generated { get; set; } = [];
    public List<SkippedOccurrence> Skipped { get; set; } = [];
}

public class SkippedOccurrence
{
    public DateOnly Date { get; set; }
    public string Reason { get; set; } = string.Empty;
}
