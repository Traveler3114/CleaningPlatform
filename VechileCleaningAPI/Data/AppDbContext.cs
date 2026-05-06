using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VechileCleaningAPI.Enums;
using VechileCleaningAPI.Entities;

namespace VechileCleaningAPI.Data;

public class AppDbContext : DbContext
{

    // DATE CONVENTION:
    // - Date fields (booking date, override date) = local Croatia time, date-only (.Date stripped)
    // - Timestamp fields (CreatedAt) = UTC (DateTime.UtcNow)
    // - No timezone conversion is done server-side; Hour is stored as an integer (0-23)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WeeklySchedule> WeeklySchedules => Set<WeeklySchedule>();
    public DbSet<DateOverride> DateOverrides => Set<DateOverride>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store BookingStatus as string
        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion(new EnumToStringConverter<BookingStatus>());

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion(new EnumToStringConverter<UserRole>());

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "owner",
            Name = "Owner",
            Surname= "User",
            PasswordHash = "$2y$10$qZKh.FlEZrHNSyAcazlNdOyBMHA.SJSfnLDoPtuFKt9Mrj99tdNEe",
            Role = UserRole.Owner,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
