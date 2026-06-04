using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("ChecklistResponses")]
public class ChecklistResponse
{
    public int BookingId { get; set; }
    public int SopTemplateId { get; set; }
    public int ChecklistItemId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public BookingSopAssignment BookingSopAssignment { get; set; } = null!;
    public ChecklistItem ChecklistItem { get; set; } = null!;
}
