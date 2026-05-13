using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("BookingSopAssignments")]
public class BookingSopAssignment
{
    [Key]
    public int Id { get; set; }

    public int BookingId { get; set; }
    public int SopTemplateId { get; set; }
    public string? CustomInstructions { get; set; }
    public DateTime AssignedAt { get; set; }

    public Booking Booking { get; set; } = null!;
    public SopTemplate SopTemplate { get; set; } = null!;
}
