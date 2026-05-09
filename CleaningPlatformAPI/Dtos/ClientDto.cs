// CleaningPlatformAPI/Dtos/ClientDto.cs

namespace CleaningPlatformAPI.Dtos;

public class ClientDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // OneTime, RepeatIndividual, RepeatBusiness
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Primary contact info
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactPhone { get; set; }
    public string? PrimaryContactEmail { get; set; }

    public int TotalBookings { get; set; }
}

public class ClientProfileDto : ClientDto
{
    public List<ContactDto> Contacts { get; set; } = new();
    public List<SiteDto> Sites { get; set; } = new();
    public List<BookingDto> Bookings { get; set; } = new();
}

public class ContactDto
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

public class SiteDto
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

public class UpdateClientProfileDto
{
    public string ClientName { get; set; } = string.Empty;
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public List<UpsertContactDto> Contacts { get; set; } = new();
}

public class UpsertContactDto
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

public class UpsertSiteDto
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
