using AirplaneSystem.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace AirplaneSystem.Infrastructure.ExternalServices.Sms;

public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _config;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly IEncryptionService _encryption;

    public TwilioSmsService(IConfiguration config, ILogger<TwilioSmsService> logger, IEncryptionService encryption)
    {
        _config = config;
        _logger = logger;
        _encryption = encryption;
    }

    public async Task SendAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        var accountSid = _config["Twilio:AccountSid"] ?? string.Empty;
        var authTokenRaw = _config["Twilio:AuthToken"] ?? string.Empty;
        var authToken = _encryption.IsEncrypted(authTokenRaw) ? _encryption.Decrypt(authTokenRaw) : authTokenRaw;
        var fromNumber = _config["Twilio:FromNumber"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
        {
            _logger.LogWarning("Twilio not configured. SMS not sent to {Phone}", phoneNumber);
            return;
        }

        TwilioClient.Init(accountSid, authToken);

        var result = await MessageResource.CreateAsync(
            body: message,
            from: new Twilio.Types.PhoneNumber(fromNumber),
            to: new Twilio.Types.PhoneNumber(phoneNumber));

        _logger.LogInformation("SMS sent to {Phone}: SID={Sid}", phoneNumber, result.Sid);
    }
}
