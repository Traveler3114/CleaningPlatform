using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;
[Table("Inventory")]
public class Inventory
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    [Required, MaxLength(20)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    [Required, MaxLength(20)]
    public string Type { get; set; } = "Consumable";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public ICollection<ServiceInventoryRequirement> ServiceRequirements { get; set; } = new List<ServiceInventoryRequirement>();
}
