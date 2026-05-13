using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("ChecklistItems")]
public class ChecklistItem
{
    [Key]
    public int Id { get; set; }

    public int SopTemplateId { get; set; }

    [Required, MaxLength(500)]
    public string ItemText { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;

    public SopTemplate SopTemplate { get; set; } = null!;
    public ICollection<ChecklistResponse> ChecklistResponses { get; set; } = new List<ChecklistResponse>();
}
