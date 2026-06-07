using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("SopTemplates")]
public class SopTemplate
{
    [Key]
    public int Id { get; set; }

    public int? ServiceCatalogId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ServiceType { get; set; } = "Generic";

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ServiceCatalog? ServiceCatalog { get; set; }
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];
    public ICollection<BookingSopAssignment> BookingSopAssignments { get; set; } = [];
}
