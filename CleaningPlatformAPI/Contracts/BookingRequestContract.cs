namespace CleaningPlatformAPI.Contracts;

public class BookingRequestResponse
{
    public int Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public string? AdminNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<BookingRequestServiceResponse> Services { get; set; } = [];
}

public class BookingRequestServiceResponse
{
    public int Id { get; set; }
    public int ServiceCatalogId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
}

public class CreateBookingRequestRequest
{
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<int> ServiceCatalogIds { get; set; } = [];
}

public class UpdateBookingRequestRequest
{
    public decimal? EstimatedPrice { get; set; }
    public string? AdminNotes { get; set; }
    public List<int> ServiceCatalogIds { get; set; } = [];
}

public class CustomerConfirmRequest
{
    public string Token { get; set; } = string.Empty;
}

public class CustomerPreviewResponse
{
    public int Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public string? AdminNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<BookingRequestServiceResponse> Services { get; set; } = [];
}
