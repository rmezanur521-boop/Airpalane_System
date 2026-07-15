namespace AirplaneSystem.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken ct = default);

    /// <summary>
    /// Sends an HTML email with one or more binary attachments (e.g. a PDF ticket).
    /// Kept as a separate method (rather than an optional parameter) so existing
    /// call sites are unaffected.
    /// </summary>
    Task SendWithAttachmentsAsync(string to, string subject, string htmlBody,
        IEnumerable<EmailAttachment> attachments, CancellationToken ct = default);
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}