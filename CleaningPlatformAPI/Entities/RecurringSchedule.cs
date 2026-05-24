using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;
[Table("RecurringSchedules")]
public class RecurringSchedule
{
    [Key]
    public int Id { get; set; }

    public int SourceBookingId { get; set; }

    public string Frequency { get; set; } = string.Empty;

    public int? DayOfWeek { get; set; }

    public int? DayOfMonth { get; set; }

    public int AutoGenerateWeeksAhead { get; set; } = 4;

    public bool IsActive { get; set; } = true;

    public DateOnly? EndsOn { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    [ForeignKey(nameof(SourceBookingId))]
    public Booking SourceBooking { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = [];
}
