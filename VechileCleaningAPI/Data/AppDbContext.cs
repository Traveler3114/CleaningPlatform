using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VechileCleaningAPI.Entities;

namespace VechileCleaningAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeeklySchedule> WeeklySchedules => Set<WeeklySchedule>();
    public DbSet<DateOverride> DateOverrides => Set<DateOverride>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store BookingStatus as string
        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion(new EnumToStringConverter<BookingStatus>());
    }
}
