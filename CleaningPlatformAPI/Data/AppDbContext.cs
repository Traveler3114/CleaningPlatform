using Microsoft.EntityFrameworkCore;
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
        public DbSet<Site> Sites { get; set; }
        public DbSet<ServiceCatalog> ServiceCatalog { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingAssignment> BookingAssignments { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<InvoiceBooking> InvoiceBookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<VehicleBookingDetails> VehicleBookingDetails { get; set; }
        public DbSet<BoatBookingDetails> BoatBookingDetails { get; set; }
        public DbSet<WeeklySchedule> WeeklySchedules { get; set; }
        public DbSet<DateOverride> DateOverrides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // Decimal precision configuration (prevents truncation warnings)
            // ============================
            modelBuilder.Entity<Employee>()
                .Property(e => e.HourlyRate)
                .HasPrecision(10, 2);

            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.PriceMin).HasPrecision(10, 2);
            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.PriceMax).HasPrecision(10, 2);
            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.PriceAvg).HasPrecision(10, 2);
            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.DefaultMarginPct).HasPrecision(5, 2);

            modelBuilder.Entity<BookingService>()
                .Property(bs => bs.EstimatedPrice).HasPrecision(10, 2);
            modelBuilder.Entity<BookingService>()
                .Property(bs => bs.FinalPrice).HasPrecision(10, 2);
            modelBuilder.Entity<BookingService>()
                .Property(bs => bs.Quantity).HasPrecision(10, 2);

            modelBuilder.Entity<Site>()
                .Property(s => s.FloorAreaM2).HasPrecision(10, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.SubTotal).HasPrecision(10, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.DiscountAmount).HasPrecision(10, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.VatPct).HasPrecision(5, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.VatAmount).HasPrecision(10, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount).HasPrecision(10, 2);

            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.Quantity).HasPrecision(10, 2);
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.UnitPrice).HasPrecision(10, 2);
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.DiscountPct).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceLine>()
                .Property(il => il.VatPct).HasPrecision(5, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount).HasPrecision(10, 2);

            modelBuilder.Entity<BoatBookingDetails>()
                .Property(b => b.LengthMeters).HasPrecision(5, 2);

            modelBuilder.Entity<Booking>()
                .Property(b => b.ServiceType)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            // ============================
            // Relationships
            // ============================

            // Employee → Role
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Role → RolePermissions
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking → VehicleBookingDetails (1-to-1)
            modelBuilder.Entity<VehicleBookingDetails>()
                .HasKey(v => v.BookingId);
            modelBuilder.Entity<VehicleBookingDetails>()
                .HasOne(v => v.Booking)
                .WithOne(b => b.VehicleDetails)
                .HasForeignKey<VehicleBookingDetails>(v => v.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking → BoatBookingDetails (1-to-1)
            modelBuilder.Entity<BoatBookingDetails>()
                .HasKey(b => b.BookingId);
            modelBuilder.Entity<BoatBookingDetails>()
                .HasOne(b => b.Booking)
                .WithOne(bk => bk.BoatDetails)
                .HasForeignKey<BoatBookingDetails>(b => b.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking → BookingServices (1-to-many)
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // BookingService → ServiceCatalog
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.ServiceCatalog)
                .WithMany(sc => sc.BookingServices)
                .HasForeignKey(bs => bs.ServiceCatalogId);

            // Client → Contacts (1-to-many)
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contacts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client → Sites (1-to-many)
            modelBuilder.Entity<Site>()
                .HasOne(s => s.Client)
                .WithMany(c => c.Sites)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingAssignment>()
                .HasOne(ba => ba.Booking)
                .WithMany(b => b.Assignments)
                .HasForeignKey(ba => ba.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingAssignment>()
                .HasOne(ba => ba.Employee)
                .WithMany(e => e.BookingAssignments)
                .HasForeignKey(ba => ba.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingAssignment>()
                .HasIndex(ba => new { ba.BookingId, ba.EmployeeId })
                .IsUnique();

            // Site → Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Site)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SiteId)
                .OnDelete(DeleteBehavior.SetNull);

            // Client → Invoices
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee → Invoices (created by)
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.CreatedByEmployee)
                .WithMany()
                .HasForeignKey(i => i.CreatedByEmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice → Lines
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(il => il.Invoice)
                .WithMany(i => i.Lines)
                .HasForeignKey(il => il.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Invoice → Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.RecordedByEmployee)
                .WithMany()
                .HasForeignKey(p => p.RecordedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice ↔ Booking junction
            modelBuilder.Entity<InvoiceBooking>()
                .HasOne(ib => ib.Invoice)
                .WithMany(i => i.InvoiceBookings)
                .HasForeignKey(ib => ib.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceBooking>()
                .HasOne(ib => ib.Booking)
                .WithMany(b => b.InvoiceBookings)
                .HasForeignKey(ib => ib.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceBooking>()
                .HasIndex(ib => ib.BookingId)
                .IsUnique();

            // Global DateTime precision (optional)
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
