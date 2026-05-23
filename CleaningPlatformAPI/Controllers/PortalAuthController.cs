using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Services;

namespace CleaningPlatformAPI.Controllers;

[ApiController]
[Route("api/portal")]
public class PortalAuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenManager _tokenManager;
    private readonly EmailService _emailService;
    private readonly IConfiguration _config;

    public PortalAuthController(
        AppDbContext db,
        TokenManager tokenManager,
        EmailService emailService,
        IConfiguration config)
    {
        _db = db;
        _tokenManager = tokenManager;
        _emailService = emailService;
        _config = config;
    }

    [HttpPost("send-link")]
    public async Task<ActionResult<OperationResult<string>>> SendLink([FromBody] SendMagicLinkRequest request)
    {
        var email = request.Email?.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(email))
            return Ok(OperationResult<string>.Ok("If the email exists, a sign-in link has been sent."));

        var contact = await _db.Contacts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == email && c.IsActive);

        if (contact != null && contact.Client.IsActive)
        {
            var token = _tokenManager.CreateMagicLinkToken(contact.ClientId, email, contact.Client.ClientName);
            var scheme = Request.Scheme;
            var host = Request.Host;
            var link = $"{scheme}://{host}/portal/magic-link.html?token={token}";

            var clientName = contact.Client.ClientName;
            var body = $"Hi {clientName},\n\n" +
                       $"Click this link to sign in to your CleanPro account:\n\n{link}\n\n" +
                       $"This link expires in 15 minutes.\n\n\u2014 CleanPro Team";

            await _emailService.SendAsync(email, "Sign in to CleanPro", body);
        }

        return Ok(OperationResult<string>.Ok("If the email exists, a sign-in link has been sent."));
    }

    [HttpPost("validate-token")]
    public async Task<ActionResult<OperationResult<string>>> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return UnprocessableEntity(OperationResult<string>.Fail("Token is required."));

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
            var result = await handler.ValidateTokenAsync(request.Token, validationParameters);

            if (!result.IsValid)
                return UnprocessableEntity(OperationResult<string>.Fail("This link has expired or is invalid. Please request a new one."));

            // Read claims from the validated JWT payload via the JsonWebToken
            var jwt = handler.ReadJsonWebToken(request.Token);
            var payloadClaims = jwt.Claims.ToList();

            var authType = payloadClaims.FirstOrDefault(c => c.Type == "auth_type")?.Value;
            var purpose = payloadClaims.FirstOrDefault(c => c.Type == "purpose")?.Value;

            if (authType != "portal" || purpose != "magic_link")
                return UnprocessableEntity(OperationResult<string>.Fail("This link is invalid. Please request a new one."));

            var clientIdClaim = payloadClaims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            var emailClaim = payloadClaims.FirstOrDefault(c => c.Type == "email")?.Value;
            var nameClaim = payloadClaims.FirstOrDefault(c => c.Type == "name")?.Value;

            if (clientIdClaim == null || emailClaim == null || nameClaim == null)
                return UnprocessableEntity(OperationResult<string>.Fail("This link is invalid. Please request a new one."));

            var clientId = int.Parse(clientIdClaim);
            var email = emailClaim;
            var name = nameClaim;

            var sessionToken = _tokenManager.CreatePortalSessionToken(clientId, email, name);
            return Ok(OperationResult<string>.Ok(sessionToken));
        }
        catch
        {
            return UnprocessableEntity(OperationResult<string>.Fail("This link has expired or is invalid. Please request a new one."));
        }
    }
}

public record SendMagicLinkRequest
{
    public string? Email { get; set; }
}

public record ValidateTokenRequest
{
    public string? Token { get; set; }
}
