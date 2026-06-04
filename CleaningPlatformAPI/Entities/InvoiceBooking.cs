using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;
[Table("InvoiceBookings")]
public class InvoiceBooking
{
    public int BookingId { get; set; }
    public int InvoiceId { get; set; }

    [ForeignKey(nameof(InvoiceId))]
    public Invoice Invoice { get; set; } = null!;

    [ForeignKey(nameof(BookingId))]
    public Booking Booking { get; set; } = null!;
}
