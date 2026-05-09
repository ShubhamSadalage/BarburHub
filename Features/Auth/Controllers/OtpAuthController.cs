using System.ComponentModel.DataAnnotations;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Shared.Config;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BarberHub.Web.Features.Auth.Controllers;

public class OtpRequestDto
{
    [Required]
    [Display(Name = "Email or phone")]
    public string Recipient { get; set; } = string.Empty;
}

public class OtpVerifyDto
{
    [Required] public string Recipient { get; set; } = string.Empty;
    [Required] public string Channel { get; set; } = "email";   // "email" or "sms"

    [Required, StringLength(8, MinimumLength = 4)]
    [Display(Name = "Verification code")]
    public string Code { get; set; } = string.Empty;
}

[Route("[controller]/[action]")]
public class OtpAuthController : Controller
{
    private readonly IOtpService _otp;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly FeatureFlags _features;

    public OtpAuthController(
        IOtpService otp,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<FeatureFlags> features)
    {
        _otp = otp;
        _userManager = userManager;
        _signInManager = signInManager;
        _features = features.Value;
    }

    /// <summary>Step 1: show the input form (email or phone).</summary>
    [HttpGet]
    public IActionResult Request()
    {
        if (!_features.EmailOtpLoginEnabled && !_features.SmsOtpLoginEnabled)
        {
            TempData["Error"] = "OTP login is currently disabled.";
            return RedirectToAction("Login", "Auth");
        }
        ViewBag.EmailOtpEnabled = _features.EmailOtpLoginEnabled;
        ViewBag.SmsOtpEnabled = _features.SmsOtpLoginEnabled;
        return View(new OtpRequestDto());
    }

    /// <summary>Step 1 submit: issue OTP, redirect to verify page.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("otp")]
    public async Task<IActionResult> Request(OtpRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EmailOtpEnabled = _features.EmailOtpLoginEnabled;
            ViewBag.SmsOtpEnabled = _features.SmsOtpLoginEnabled;
            return View(dto);
        }

        var input = dto.Recipient.Trim();
        var isEmail = input.Contains('@');
        var channel = isEmail ? OtpChannel.Email : OtpChannel.Sms;

        // Honor feature flags
        if (channel == OtpChannel.Email && !_features.EmailOtpLoginEnabled)
        {
            TempData["Error"] = "Email OTP is disabled.";
            return RedirectToAction(nameof(Request));
        }
        if (channel == OtpChannel.Sms && !_features.SmsOtpLoginEnabled)
        {
            TempData["Error"] = "SMS OTP is disabled.";
            return RedirectToAction(nameof(Request));
        }

        // We do NOT reveal whether the account exists. Send code regardless;
        // verification step will deny login if no user matches.
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _otp.IssueAsync(input, channel, OtpPurpose.Login, ip);

        if (!result.Sent)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not send code.");
            ViewBag.EmailOtpEnabled = _features.EmailOtpLoginEnabled;
            ViewBag.SmsOtpEnabled = _features.SmsOtpLoginEnabled;
            return View(dto);
        }

        TempData["Success"] = isEmail
            ? $"Code sent to {input}. Check your inbox."
            : $"Code sent to {input}.";
        if (!string.IsNullOrEmpty(result.DebugCode))
        {
            TempData["Success"] += $" (Test mode code: {result.DebugCode})";
        }

        return RedirectToAction(nameof(Verify), new { recipient = input, channel = isEmail ? "email" : "sms" });
    }

    [HttpGet]
    public IActionResult Verify(string recipient, string channel)
    {
        if (string.IsNullOrEmpty(recipient)) return RedirectToAction(nameof(Request));
        return View(new OtpVerifyDto { Recipient = recipient, Channel = channel ?? "email" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(OtpVerifyDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var channel = string.Equals(dto.Channel, "sms", StringComparison.OrdinalIgnoreCase)
            ? OtpChannel.Sms : OtpChannel.Email;

        var v = await _otp.VerifyAsync(dto.Recipient, dto.Code, OtpPurpose.Login, channel);
        if (!v.Ok)
        {
            ModelState.AddModelError(string.Empty, v.Error ?? "Verification failed.");
            return View(dto);
        }

        if (string.IsNullOrEmpty(v.UserId))
        {
            ModelState.AddModelError(string.Empty,
                "No account found for this " + (channel == OtpChannel.Email ? "email" : "phone") +
                ". Please register first.");
            return View(dto);
        }

        var user = await _userManager.FindByIdAsync(v.UserId);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            ModelState.AddModelError(string.Empty, "Account is not active.");
            return View(dto);
        }

        // Sign in (no password — we already verified via OTP)
        await _signInManager.SignInAsync(user, isPersistent: true);

        // Confirm phone if it was an SMS OTP and the phone matches the account
        if (channel == OtpChannel.Sms && user.PhoneNumber == dto.Recipient && !user.PhoneNumberConfirmed)
        {
            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        TempData["Success"] = $"Welcome back, {user.FullName}!";
        return RedirectToAction("Index", "Home");
    }
}
