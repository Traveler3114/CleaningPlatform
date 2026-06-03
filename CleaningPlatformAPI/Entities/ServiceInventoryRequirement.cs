using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;
[Table("ServiceInventoryRequirements")]
public class ServiceInventoryRequirement
{
    [Key]
    public int Id { get; set; }

    public int ServiceCatalogId { get; set; }
    public int InventoryId { get; set; }

    public decimal QuantityNeeded { get; set; }

    [ForeignKey(nameof(ServiceCatalogId))]
    public ServiceCatalog ServiceCatalog { get; set; } = null!;

    [ForeignKey(nameof(InventoryId))]
    public Inventory Inventory { get; set; } = null!;
}
