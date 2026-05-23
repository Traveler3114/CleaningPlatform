using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class EmployeeUtilizationView
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int JobsAssigned { get; set; }
    public int JobsCompleted { get; set; }
    public int DaysActive { get; set; }
    public decimal CompletionRatePct { get; set; }
}
