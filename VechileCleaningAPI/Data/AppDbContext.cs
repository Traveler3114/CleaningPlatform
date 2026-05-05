using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VechileCleaningAPI.Entities;

namespace VechileCleaningAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeeklySchedule> WeeklySchedules => Set<WeeklySchedule>();
    public DbSet<HourOverride> HourOverrides => Set<HourOverride>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store BookingStatus as string
        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion(new EnumToStringConverter<BookingStatus>());

        // Seed WeeklySchedule
        modelBuilder.Entity<WeeklySchedule>().HasData(
            new WeeklySchedule { Id = 1, DayOfWeek = 0, IsClosed = true,  StartHour = 8, EndHour = 17},
            new WeeklySchedule { Id = 2, DayOfWeek = 1, IsClosed = false, StartHour = 8, EndHour = 17},
            new WeeklySchedule { Id = 3, DayOfWeek = 2, IsClosed = false, StartHour = 8, EndHour = 17},
            new WeeklySchedule { Id = 4, DayOfWeek = 3, IsClosed = false, StartHour = 8, EndHour = 17},
            new WeeklySchedule { Id = 5, DayOfWeek = 4, IsClosed = false, StartHour = 8, EndHour = 17},
            new WeeklySchedule { Id = 6, DayOfWeek = 5, IsClosed = false, StartHour = 8, EndHour = 17},
            new WeeklySchedule { Id = 7, DayOfWeek = 6, IsClosed = false, StartHour = 9, EndHour = 13}
        );
    }
}
