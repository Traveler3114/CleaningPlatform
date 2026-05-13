using Microsoft.EntityFrameworkCore;

namespace CleaningPlatformAPI.Entities;

[Keyless]
public class JobCompletionRateView
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalJobs { get; set; }
    public int CompletedJobs { get; set; }
    public double? CompletionRatePct { get; set; }
}
