// CleaningPlatformAPI/Dtos/CreateClientDto.cs

namespace CleaningPlatformAPI.Dtos;

public class CreateClientDto
{
    public string ClientName { get; set; } = string.Empty;
    public string Type { get; set; } = "RepeatIndividual"; // RepeatIndividual or RepeatBusiness
    public string? Oib { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }

    public string PrimaryContactName { get; set; } = string.Empty;
    public string PrimaryContactPhone { get; set; } = string.Empty;
    public string? PrimaryContactEmail { get; set; }
}