using BarberHub.Web.Shared.Config;
using Microsoft.Extensions.Options;

namespace BarberHub.Web.Shared.Services;

/// <summary>
/// Pluggable SMS sender. Set Sms.Provider in appsettings.json to "Console" (default,
/// logs the SMS to console — great for local testing), "MSG91", "Twilio", or "Fast2SMS".
/// Real provider implementations call out to the provider's REST API.
/// </summary>
public interface ISmsService
{
    /// <summary>Sends an SMS. Returns true on success; false on configuration / API error.</summary>
    Task<bool> SendAsync(string toPhoneNumber, string message);
}

public class SmsService : ISmsService
{
    private readonly SmsOptions _options;
    private readonly ILogger<SmsService> _logger;
    private readonly IHttpClientFactory _httpFactory;

    public SmsService(IOptions<SmsOptions> options, ILogger<SmsService> logger, IHttpClientFactory httpFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpFactory = httpFactory;
    }

    public async Task<bool> SendAsync(string toPhoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber)) return false;
        var phone = NormalizePhone(toPhoneNumber);

        try
        {
            return _options.Provider?.ToUpperInvariant() switch
            {
                "MSG91"   => await SendViaMsg91Async(phone, message),
                "TWILIO"  => await SendViaTwilioAsync(phone, message),
                "FAST2SMS"=> await SendViaFast2SmsAsync(phone, message),
                _         => SendViaConsole(phone, message)   // default
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS send failed via provider {Provider}", _options.Provider);
            return false;
        }
    }

    private bool SendViaConsole(string phone, string message)
    {
        // Local-dev mode. Code goes into application logs so you can copy it.
        _logger.LogWarning("SMS [Console] to {Phone}: {Message}", phone, message);
        return true;
    }

    // --- Provider stubs — fill in API key and uncomment the HTTP call when going live. ---

    private async Task<bool> SendViaMsg91Async(string phone, string message)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("MSG91 SMS skipped: ApiKey not configured. Falling back to Console.");
            return SendViaConsole(phone, message);
        }
        // MSG91 OTP API — replace TemplateId-based call as needed
        var url = $"https://api.msg91.com/api/v5/flow/?authkey={_options.ApiKey}";
        // Pseudocode placeholder — implement once you have an active MSG91 account & template
        var http = _httpFactory.CreateClient();
        // Real call would POST a JSON body with mobiles=phone, template_id=..., var=otp
        await Task.CompletedTask;
        _logger.LogInformation("MSG91 stub — pretend-sent to {Phone}", phone);
        return true;
    }

    private async Task<bool> SendViaTwilioAsync(string phone, string message)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Twilio SMS skipped: ApiKey not configured. Falling back to Console.");
            return SendViaConsole(phone, message);
        }
        // Real Twilio integration: install Twilio NuGet package and use TwilioClient.
        await Task.CompletedTask;
        _logger.LogInformation("Twilio stub — pretend-sent to {Phone}", phone);
        return true;
    }

    private async Task<bool> SendViaFast2SmsAsync(string phone, string message)
    {
        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            _logger.LogWarning("Fast2SMS skipped: ApiKey not configured. Falling back to Console.");
            return SendViaConsole(phone, message);
        }
        // Real Fast2SMS: GET https://www.fast2sms.com/dev/bulkV2?authorization=...&route=otp&...
        await Task.CompletedTask;
        _logger.LogInformation("Fast2SMS stub — pretend-sent to {Phone}", phone);
        return true;
    }

    private static string NormalizePhone(string phone)
    {
        // Keep digits and a leading '+'. Indian default: prepend +91 if 10 digits.
        var trimmed = (phone ?? "").Trim();
        if (string.IsNullOrEmpty(trimmed)) return trimmed;
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (trimmed.StartsWith("+")) return "+" + digits;
        if (digits.Length == 10) return "+91" + digits;
        return "+" + digits;
    }
}
