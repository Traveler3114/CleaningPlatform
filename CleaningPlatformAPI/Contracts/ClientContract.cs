namespace CleaningPlatformAPI.Contracts;

public class ClientResponse
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactPhone { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public int TotalBookings { get; set; }

    public List<ContactResponse>? Contacts { get; set; }
    public List<SiteResponse>? Sites { get; set; }
    public List<BookingResponse>? Bookings { get; set; }
}

public class ContactResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
}

public class SiteResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? SiteType { get; set; }
    public decimal? FloorAreaM2 { get; set; }
    public string? AccessNotes { get; set; }
    public bool IsActive { get; set; }
}

public record CreateClientRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string Type { get; set; } = "RepeatIndividual";
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string PrimaryContactName { get; set; } = string.Empty;
    public string PrimaryContactPhone { get; set; } = string.Empty;
    public string? PrimaryContactEmail { get; set; }
}

public record UpdateClientProfileRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public List<UpsertContactRequest> Contacts { get; set; } = new();
}

public record UpsertContactRequest
{
    public int? Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}

public record UpsertSiteRequest
{
    public string SiteName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? SiteType { get; set; }
    public decimal? FloorAreaM2 { get; set; }
    public string? AccessNotes { get; set; }
    public bool IsActive { get; set; } = true;
}
