namespace CleaningPlatformAPI.Contracts;

public record SendMagicLinkRequest
{
    public string? Email { get; set; }
}

public record ValidateTokenRequest
{
    public string? Token { get; set; }
}
