using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("BookingAssignments")]
public class BookingAssignment
{
    [Key]
    public int Id { get; set; }

    public int BookingId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignedAt { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking Booking { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    public ICollection<ChecklistResponse> ChecklistResponses { get; set; } = new List<ChecklistResponse>();
}
