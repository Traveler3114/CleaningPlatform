using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("BookingRequests")]
public class BookingRequest
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string ContactName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public decimal? EstimatedPrice { get; set; }

    public string? AdminNotes { get; set; }

    [Required, MaxLength(50)]
    public string Status { get; set; } = "New";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<BookingRequestService> RequestServices { get; set; } = [];
}
