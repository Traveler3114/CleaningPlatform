using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleaningPlatformAPI.Entities;

[Table("BookingRequestServices")]
public class BookingRequestService
{
    public int BookingRequestId { get; set; }
    public int ServiceCatalogId { get; set; }

    [ForeignKey(nameof(BookingRequestId))]
    public BookingRequest BookingRequest { get; set; } = null!;

    [ForeignKey(nameof(ServiceCatalogId))]
    public ServiceCatalog ServiceCatalog { get; set; } = null!;
}
