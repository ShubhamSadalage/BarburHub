namespace BarberHub.Web.Domain.Entities;

public enum OtpPurpose
{
    Login = 0,           // Email OTP login (passwordless or as 2FA step)
    PasswordReset = 1,
    SuperAdmin2Fa = 2,
    PhoneVerification = 3
}

public class OtpCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>UserId if known; null for OTP-by-email flows where we don't reveal user existence.</summary>
    public string? UserId { get; set; }

    /// <summary>Email or phone the OTP was sent to.</summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>SHA-256 hash of the 6-digit code (we never store plaintext).</summary>
    public string CodeHash { get; set; } = string.Empty;

    public OtpPurpose Purpose { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? IpAddress { get; set; }
}
