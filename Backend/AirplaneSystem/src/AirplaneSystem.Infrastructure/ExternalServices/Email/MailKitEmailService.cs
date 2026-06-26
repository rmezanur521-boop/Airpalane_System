using AirplaneSystem.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AirplaneSystem.Infrastructure.ExternalServices.Email;

public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailKitEmailService> _logger;
    private readonly IEncryptionService _encryption;

    public MailKitEmailService(IConfiguration config, ILogger<MailKitEmailService> logger, IEncryptionService encryption)
    {
        _config = config;
        _logger = logger;
        _encryption = encryption;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default) =>
        await SendAsync(new[] { to }, subject, htmlBody, ct);

    public async Task SendAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken ct = default)
    {
        var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(_config["Smtp:Port"] ?? "587");
        var username = _config["Smtp:Username"] ?? string.Empty;
        var passwordRaw = _config["Smtp:Password"] ?? string.Empty;
        var password = _encryption.IsEncrypted(passwordRaw) ? _encryption.Decrypt(passwordRaw) : passwordRaw;
        var fromName = _config["Smtp:FromName"] ?? "AirSystem";
        var fromEmail = _config["Smtp:FromEmail"] ?? username;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        foreach (var r in recipients)
            message.To.Add(MailboxAddress.Parse(r));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
            _logger.LogInformation("Email sent to {Recipients}: {Subject}", string.Join(", ", recipients), subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", recipients));
            throw;
        }
    }
}
