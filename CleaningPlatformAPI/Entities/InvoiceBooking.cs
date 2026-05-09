using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities
{
    [Table("InvoiceBookings")]
    public class InvoiceBooking
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public int BookingId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public Invoice Invoice { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking Booking { get; set; }
    }
}
