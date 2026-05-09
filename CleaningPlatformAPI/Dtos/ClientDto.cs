// CleaningPlatformAPI/Dtos/ClientDto.cs

namespace CleaningPlatformAPI.Dtos;

public class ClientDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // OneTime, RepeatIndividual, RepeatBusiness
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
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