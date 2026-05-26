using SendGrid;
using SendGrid.Helpers.Mail;

namespace CleaningPlatformAPI.Services;

public class EmailService
{
    private readonly string? _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        var section = config.GetSection("SendGrid");
        _apiKey = section["ApiKey"];
        _fromEmail = section["FromEmail"] ?? "noreply@cleaningplatform.com";
        _fromName = section["FromName"] ?? "CleanPro";
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("SendGrid API key is not configured. Email to {To} was not sent.", to);
            return;
        }

        try
        {
            var client = new SendGridClient(_apiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmail, _fromName),
                Subject = subject,
                PlainTextContent = body
            };
            msg.AddTo(to);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var bodyText = await response.Body.ReadAsStringAsync();
                _logger.LogWarning("SendGrid returned {StatusCode} for email to {To}: {Body}",
                    response.StatusCode, to, bodyText);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }
}
