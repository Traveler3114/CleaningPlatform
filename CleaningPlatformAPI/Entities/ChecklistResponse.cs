using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("ChecklistResponses")]
public class ChecklistResponse
{
    [Key]
    public int Id { get; set; }

    public int BookingAssignmentId { get; set; }
    public int ChecklistItemId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public BookingAssignment BookingAssignment { get; set; } = null!;
    public ChecklistItem ChecklistItem { get; set; } = null!;
}
