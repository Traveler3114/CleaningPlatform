using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Data;

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
    public DbSet<Employee> Users => Set<Employee>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store BookingStatus as string
        modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion(new EnumToStringConverter<Enums.BookingStatus>());

        // Role entity
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .HasMaxLength(100);

        // RolePermission entity
        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.PermissionKey)
            .HasMaxLength(100);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed owner user
        modelBuilder.Entity<Employee>().HasData(new Employee
        {
            Id = 1,
            Username = "owner",
            Name = "Owner",
            Surname = "User",
            PasswordHash = "$2y$10$qZKh.FlEZrHNSyAcazlNdOyBMHA.SJSfnLDoPtuFKt9Mrj99tdNEe",
            RoleName = "Owner",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Owner",      IsProtected = true,  CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "Dispatcher", IsProtected = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 3, Name = "Cleaner",    IsProtected = false, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed role permissions
        modelBuilder.Entity<RolePermission>().HasData(
            // Owner — all permissions
            new RolePermission { Id = 1,  RoleId = 1, PermissionKey = PermissionKeys.PagesDaily },
            new RolePermission { Id = 2,  RoleId = 1, PermissionKey = PermissionKeys.PagesBookings },
            new RolePermission { Id = 3,  RoleId = 1, PermissionKey = PermissionKeys.PagesSchedule },
            new RolePermission { Id = 4,  RoleId = 1, PermissionKey = PermissionKeys.PagesUsers },
            new RolePermission { Id = 5,  RoleId = 1, PermissionKey = PermissionKeys.PagesRoles },
            new RolePermission { Id = 6,  RoleId = 1, PermissionKey = PermissionKeys.ActionsBookingUpdateStatus },
            new RolePermission { Id = 7,  RoleId = 1, PermissionKey = PermissionKeys.ActionsScheduleEdit },
            new RolePermission { Id = 8,  RoleId = 1, PermissionKey = PermissionKeys.ActionsOverrideManage },
            new RolePermission { Id = 9,  RoleId = 1, PermissionKey = PermissionKeys.ActionsUserCreate },
            new RolePermission { Id = 10, RoleId = 1, PermissionKey = PermissionKeys.ActionsUserToggleActive },
            new RolePermission { Id = 11, RoleId = 1, PermissionKey = PermissionKeys.ActionsRoleManage },
            // Dispatcher
            new RolePermission { Id = 12, RoleId = 2, PermissionKey = PermissionKeys.PagesDaily },
            new RolePermission { Id = 13, RoleId = 2, PermissionKey = PermissionKeys.PagesBookings },
            new RolePermission { Id = 14, RoleId = 2, PermissionKey = PermissionKeys.PagesSchedule },
            new RolePermission { Id = 15, RoleId = 2, PermissionKey = PermissionKeys.ActionsBookingUpdateStatus },
            new RolePermission { Id = 16, RoleId = 2, PermissionKey = PermissionKeys.ActionsScheduleEdit },
            new RolePermission { Id = 17, RoleId = 2, PermissionKey = PermissionKeys.ActionsOverrideManage },
            // Cleaner
            new RolePermission { Id = 18, RoleId = 3, PermissionKey = PermissionKeys.PagesDaily },
            new RolePermission { Id = 19, RoleId = 3, PermissionKey = PermissionKeys.PagesBookings }
        );
    }
}
