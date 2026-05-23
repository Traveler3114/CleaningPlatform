using SendGrid;
using SendGrid.Helpers.Mail;

namespace CleaningPlatformAPI.Services;

public class EmailService
{
    private readonly string? _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration config)
    {
        var section = config.GetSection("SendGrid");
        _apiKey = section["ApiKey"];
        _fromEmail = section["FromEmail"] ?? "noreply@cleaningplatform.com";
        _fromName = section["FromName"] ?? "CleanPro";
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            var client = new SendGridClient(_apiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_fromEmail, _fromName),
                Subject = subject,
                PlainTextContent = body
            };
            msg.AddTo(to);
            await client.SendEmailAsync(msg);
        }
        else
        {
            Console.WriteLine("");
            Console.WriteLine("--- EMAIL ---");
            Console.WriteLine($"To:      {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body:");
            Console.WriteLine(body);
            Console.WriteLine("--- END EMAIL ---");
            Console.WriteLine("");
        }
    }
}
