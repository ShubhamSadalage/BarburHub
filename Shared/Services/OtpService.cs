using System.Security.Cryptography;
using System.Text;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BarberHub.Web.Shared.Services;

public enum OtpChannel { Email, Sms }

public class OtpIssueResult
{
    public bool Sent { get; set; }
    public string? Error { get; set; }
    public DateTime? ExpiresAt { get; set; }
    /// <summary>Test/dev only — the plaintext code, when running with the Console SMS provider so you can copy it.</summary>
    public string? DebugCode { get; set; }
}

public class OtpVerifyResult
{
    public bool Ok { get; set; }
    public string? Error { get; set; }
    /// <summary>UserId of the matching user (if any). Null if no account exists for this recipient.</summary>
    public string? UserId { get; set; }
}

public interface IOtpService
{
    Task<OtpIssueResult> IssueAsync(string recipient, OtpChannel channel, OtpPurpose purpose, string? ipAddress = null);
    Task<OtpVerifyResult> VerifyAsync(string recipient, string code, OtpPurpose purpose, OtpChannel channel);
}

public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _db;
    private readonly OtpOptions _otp;
    private readonly RateLimitOptions _rate;
    private readonly SmsOptions _smsOpts;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly ILogger<OtpService> _logger;

    public OtpService(
        ApplicationDbContext db,
        IOptions<OtpOptions> otp,
        IOptions<RateLimitOptions> rate,
        IOptions<SmsOptions> smsOpts,
        IEmailService email,
        ISmsService sms,
        ILogger<OtpService> logger)
    {
        _db = db;
        _otp = otp.Value;
        _rate = rate.Value;
        _smsOpts = smsOpts.Value;
        _email = email;
        _sms = sms;
        _logger = logger;
    }

    public async Task<OtpIssueResult> IssueAsync(string recipient, OtpChannel channel, OtpPurpose purpose, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            return new OtpIssueResult { Sent = false, Error = "Recipient is required." };

        recipient = recipient.Trim();
        var normalized = channel == OtpChannel.Email ? recipient.ToLowerInvariant() : recipient;

        // Rate limit: max N OTPs per hour for this recipient
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentCount = await _db.OtpCodes
            .CountAsync(o => o.Recipient == normalized && o.Purpose == purpose && o.CreatedAt >= oneHourAgo);
        if (recentCount >= _rate.OtpPerHour)
            return new OtpIssueResult { Sent = false, Error = "Too many requests. Please try again in an hour." };

        // Resend cooldown — block if last code created within the cooldown window
        var cooldownAgo = DateTime.UtcNow.AddSeconds(-_otp.ResendCooldownSeconds);
        var recent = await _db.OtpCodes
            .Where(o => o.Recipient == normalized && o.Purpose == purpose && o.CreatedAt >= cooldownAgo)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
        if (recent is not null)
        {
            var wait = (int)(_otp.ResendCooldownSeconds - (DateTime.UtcNow - recent.CreatedAt).TotalSeconds);
            return new OtpIssueResult { Sent = false, Error = $"Please wait {Math.Max(1, wait)}s before requesting another code." };
        }

        // Invalidate any unused/unexpired codes for the same recipient+purpose
        var stale = await _db.OtpCodes
            .Where(o => o.Recipient == normalized && o.Purpose == purpose && o.ConsumedAt == null && o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
        foreach (var s in stale) s.ConsumedAt = DateTime.UtcNow;

        // Generate code
        var code = GenerateNumericCode(_otp.CodeLength);
        var hash = HashCode(code, normalized);
        var expiresAt = DateTime.UtcNow.AddMinutes(_otp.ExpiryMinutes);

        var entity = new OtpCode
        {
            Recipient = normalized,
            CodeHash = hash,
            Purpose = purpose,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress
        };
        await _db.OtpCodes.AddAsync(entity);

        // Try to attach a UserId if the account exists (helps for audit; not required)
        var lookup = channel == OtpChannel.Email
            ? await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedEmail == normalized.ToUpperInvariant())
            : await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PhoneNumber == normalized);
        if (lookup is not null) entity.UserId = lookup.Id;

        await _db.SaveChangesAsync();

        // Deliver
        bool delivered;
        if (channel == OtpChannel.Email)
        {
            var html = $@"
                <p>Your Barber Hub verification code is:</p>
                <p style=""font-size:28px;font-weight:700;letter-spacing:6px;color:#ef5b5b;"">{code}</p>
                <p>This code expires in {_otp.ExpiryMinutes} minutes.</p>
                <p>If you didn't request this, you can safely ignore this email.</p>";
            try
            {
                await _email.SendAsync(normalized, "Your Barber Hub verification code", html);
                delivered = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email OTP send failed");
                delivered = false;
            }
        }
        else
        {
            delivered = await _sms.SendAsync(normalized, $"Your Barber Hub code is {code}. Valid {_otp.ExpiryMinutes} min.");
        }

        return new OtpIssueResult
        {
            Sent = delivered,
            Error = delivered ? null : "Could not deliver code. Please try again.",
            ExpiresAt = expiresAt,
            // Surface the code only when the SMS provider is "Console" — for local testing
            DebugCode = (channel == OtpChannel.Sms && _smsOpts.Provider?.Equals("Console", StringComparison.OrdinalIgnoreCase) == true)
                        ? code : null
        };
    }

    public async Task<OtpVerifyResult> VerifyAsync(string recipient, string code, OtpPurpose purpose, OtpChannel channel)
    {
        if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(code))
            return new OtpVerifyResult { Ok = false, Error = "Code is required." };

        var normalized = channel == OtpChannel.Email ? recipient.Trim().ToLowerInvariant() : recipient.Trim();

        var entity = await _db.OtpCodes
            .Where(o => o.Recipient == normalized && o.Purpose == purpose && o.ConsumedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (entity is null)
            return new OtpVerifyResult { Ok = false, Error = "No active code found. Please request a new one." };

        if (entity.ExpiresAt < DateTime.UtcNow)
            return new OtpVerifyResult { Ok = false, Error = "Code expired. Please request a new one." };

        if (entity.AttemptCount >= _otp.MaxAttempts)
        {
            entity.ConsumedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return new OtpVerifyResult { Ok = false, Error = "Too many attempts. Please request a new code." };
        }

        var hash = HashCode(code.Trim(), normalized);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(hash),
                Encoding.ASCII.GetBytes(entity.CodeHash)))
        {
            entity.AttemptCount++;
            await _db.SaveChangesAsync();
            return new OtpVerifyResult { Ok = false, Error = "Incorrect code." };
        }

        entity.ConsumedAt = DateTime.UtcNow;

        // Resolve user
        var user = channel == OtpChannel.Email
            ? await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedEmail == normalized.ToUpperInvariant())
            : await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PhoneNumber == normalized);

        await _db.SaveChangesAsync();

        return new OtpVerifyResult { Ok = true, UserId = user?.Id };
    }

    private static string GenerateNumericCode(int length)
    {
        // Cryptographically random numeric code
        Span<byte> buf = stackalloc byte[length];
        RandomNumberGenerator.Fill(buf);
        var sb = new StringBuilder(length);
        foreach (var b in buf) sb.Append((b % 10).ToString());
        return sb.ToString();
    }

    private static string HashCode(string code, string recipient)
    {
        // Salt the hash with the recipient so leaked hashes can't be cross-reused.
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{recipient}:{code}"));
        return Convert.ToHexString(bytes);
    }
}
