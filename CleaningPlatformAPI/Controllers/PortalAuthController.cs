using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var link = $"{scheme}://{host}/api/portal/auth?token={token}";

            var clientName = contact.Client.ClientName;
            var body = $"Hi {clientName},\n\n" +
                       $"Click this link to sign in to your CleanPro account:\n\n{link}\n\n" +
                       $"This link expires in 15 minutes.\n\n\u2014 CleanPro Team";

            await _emailService.SendAsync(email, "Sign in to CleanPro", body);
        }

        return Ok(OperationResult<string>.Ok("If the email exists, a sign-in link has been sent."));
    }

    [HttpGet("auth")]
    public async Task<IActionResult> Auth([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Content(ErrorPage("Missing token. Please request a new sign-in link."), "text/html");

        try
        {
            var handler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var result = await handler.ValidateTokenAsync(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            });

            if (!result.IsValid)
                return Content(ErrorPage("This link has expired or is invalid. Please request a new one."), "text/html");

            var principal = new System.Security.Claims.ClaimsPrincipal(result.ClaimsIdentity);

            var authType = principal.FindFirst("auth_type")?.Value;
            var purpose = principal.FindFirst("purpose")?.Value;

            if (authType != "portal" || purpose != "magic_link")
                return Content(ErrorPage("This link is invalid. Please request a new one."), "text/html");

            var clientId = int.Parse(principal.FindFirst("client_id")!.Value);
            var email = principal.FindFirst("email")!.Value;
            var name = principal.FindFirst("name")!.Value;

            var sessionToken = _tokenManager.CreatePortalSessionToken(clientId, email, name);

            return Content(SuccessPage(sessionToken), "text/html");
        }
        catch
        {
            return Content(ErrorPage("This link has expired or is invalid. Please request a new one."), "text/html");
        }
    }

    private static string ErrorPage(string message)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" /><title>Sign In — CleanPro</title>
<style>
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f4f6f9; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; padding: 1rem; }}
  .card {{ background: white; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); padding: 2rem; max-width: 420px; text-align: center; }}
  h1 {{ color: #e53935; font-size: 1.3rem; margin-bottom: 0.5rem; }}
  p {{ color: #666; font-size: 0.9rem; margin-bottom: 1.25rem; }}
  a {{ display: inline-block; background: #1a237e; color: white; padding: 0.6rem 1.5rem; border-radius: 6px; text-decoration: none; font-size: 0.88rem; }}
</style></head>
<body><div class=""card""><h1>Link Expired</h1><p>{message}</p><a href=""/portal/login.html"">Request New Link</a></div></body></html>";
    }

    private static string SuccessPage(string sessionToken)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" /><title>Signing in...</title>
<style>
  body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #f4f6f9; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; }}
  .spinner {{ width: 30px; height: 30px; border: 3px solid #e8eaf6; border-top-color: #1a237e; border-radius: 50%; animation: spin 0.7s linear infinite; margin: 0 auto 1rem; }}
  @@keyframes spin {{ to {{ transform: rotate(360deg); }} }}
  p {{ color: #666; font-size: 0.88rem; }}
</style></head>
<body><div style=""text-align:center;""><div class=""spinner""></div><p>Signing you in...</p></div>
<script>
  localStorage.setItem('portalSession', '{sessionToken}');
  window.location.replace('/portal/index.html');
</script>
</body></html>";
    }
}

public record SendMagicLinkRequest
{
    public string? Email { get; set; }
}
