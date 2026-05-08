using Microsoft.EntityFrameworkCore;
using System;
using CleaningPlatformAPI.Entities;

namespace CleaningPlatformAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ServiceCatalog> ServiceCatalog { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<VehicleBookingDetails> VehicleBookingDetails { get; set; }
        public DbSet<SiteDetail> SiteDetail { get; set; }
        public DbSet<BoatBookingDetails> BoatBookingDetails { get; set; }
        public DbSet<BookingView> BookingView { get; set; }
        public DbSet<WeeklySchedule> WeeklySchedules { get; set; }
        public DbSet<DateOverride> DateOverrides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Booking → VehicleBookingDetails (1-to-1) ===
            modelBuilder.Entity<VehicleBookingDetails>()
                .HasKey(v => v.BookingId);
            modelBuilder.Entity<VehicleBookingDetails>()
                .HasOne(v => v.Booking)
                .WithOne(b => b.VehicleDetails)
                .HasForeignKey<VehicleBookingDetails>(v => v.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Booking → SiteDetail (1-to-1) ===
            modelBuilder.Entity<SiteDetail>()
                .HasKey(s => s.BookingId);
            modelBuilder.Entity<SiteDetail>()
                .HasOne(s => s.Booking)
                .WithOne(b => b.SiteDetail)
                .HasForeignKey<SiteDetail>(s => s.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Booking → BoatBookingDetails (1-to-1) ===
            modelBuilder.Entity<BoatBookingDetails>()
                .HasKey(b => b.BookingId);
            modelBuilder.Entity<BoatBookingDetails>()
                .HasOne(b => b.Booking)
                .WithOne(bk => bk.BoatDetails)
                .HasForeignKey<BoatBookingDetails>(b => b.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Booking → BookingServices (1-to-many) ===
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // === BookingService → ServiceCatalog ===
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.ServiceCatalog)
                .WithMany(sc => sc.BookingServices)
                .HasForeignKey(bs => bs.ServiceCatalogId);

            // === Client → Contacts (1-to-many) ===
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contacts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Employee → Bookings ===
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.AssignedEmployee)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.AssignedEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // === View (keyless) ===
            modelBuilder.Entity<BookingView>().ToView("vw_Bookings").HasNoKey();

            // === Global DateTime precision (optional) ===
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("datetime2");
                    }
                }
            }
        }
    }
}
