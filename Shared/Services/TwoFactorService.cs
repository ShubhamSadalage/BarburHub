using System.Text;
using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Shared.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OtpNet;
using QRCoder;

namespace BarberHub.Web.Shared.Services;

public interface ITwoFactorService
{
    bool IsRequiredFor(ApplicationUser user, IList<string> roles);
    /// <summary>Generates a new TOTP secret and persists it on the user (replaces any existing).</summary>
    Task<TwoFactorSetupDto> EnrollAsync(ApplicationUser user, string issuer);
    bool VerifyCode(string base32Secret, string code);
    /// <summary>Marks 2FA enabled (call after the first successful verify in setup).</summary>
    Task ConfirmEnabledAsync(ApplicationUser user);
    Task DisableAsync(ApplicationUser user);
}

public class TwoFactorSetupDto
{
    public string Secret { get; set; } = string.Empty;            // base32, what the user types if scanning fails
    public string OtpAuthUri { get; set; } = string.Empty;
    public string QrCodePngDataUri { get; set; } = string.Empty;  // ready for <img src=...>
}

public class TwoFactorService : ITwoFactorService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FeatureFlags _features;

    public TwoFactorService(UserManager<ApplicationUser> userManager, IOptions<FeatureFlags> features)
    {
        _userManager = userManager;
        _features = features.Value;
    }

    public bool IsRequiredFor(ApplicationUser user, IList<string> roles)
    {
        if (!_features.SuperAdminTwoFactorEnabled) return false;
        return roles.Contains(AppRoles.SuperAdmin);
    }

    public async Task<TwoFactorSetupDto> EnrollAsync(ApplicationUser user, string issuer)
    {
        // Reset any previous authenticator key, then issue a new one via Identity
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var secret = await _userManager.GetAuthenticatorKeyAsync(user)
                     ?? throw new InvalidOperationException("Could not generate authenticator key.");

        var label = Uri.EscapeDataString($"{issuer}:{user.Email}");
        var otpAuth = $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30";

        // QR code → PNG → data URI
        using var qr = new QRCodeGenerator();
        using var data = qr.CreateQrCode(otpAuth, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data).GetGraphic(8);
        var dataUri = "data:image/png;base64," + Convert.ToBase64String(png);

        return new TwoFactorSetupDto
        {
            Secret = secret,
            OtpAuthUri = otpAuth,
            QrCodePngDataUri = dataUri
        };
    }

    public bool VerifyCode(string base32Secret, string code)
    {
        if (string.IsNullOrWhiteSpace(base32Secret) || string.IsNullOrWhiteSpace(code)) return false;
        // Strip spaces the user might have typed
        var clean = new string(code.Where(char.IsDigit).ToArray());
        if (clean.Length != 6) return false;
        var totp = new Totp(Base32Encoding.ToBytes(base32Secret));
        // Allow ±1 step (30s) for clock skew
        return totp.VerifyTotp(clean, out _, new VerificationWindow(1, 1));
    }

    public async Task ConfirmEnabledAsync(ApplicationUser user)
    {
        await _userManager.SetTwoFactorEnabledAsync(user, true);
    }

    public async Task DisableAsync(ApplicationUser user)
    {
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
    }
}
