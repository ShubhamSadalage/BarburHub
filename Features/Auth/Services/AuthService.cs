using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Auth.Dtos;
using BarberHub.Web.Shared;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Identity;

namespace BarberHub.Web.Features.Auth.Services;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterDto dto);
    /// <summary>
    /// Creates the user account after both OTPs have been verified by the controller.
    /// Returns the created user on success.
    /// </summary>
    Task<Result<ApplicationUser>> CompleteRegistrationAsync(RegisterDto dto);
    Task<Result<ApplicationUser>> LoginAsync(LoginDto dto);
    Task LogoutAsync();
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordDto dto);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<Result> UpdateProfileAsync(string userId, EditProfileDto dto, string? imagePath);
    Task<ApplicationUser?> GetByIdAsync(string userId);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Pre-flight validation only. Does NOT create the account — that happens in
    /// CompleteRegistrationAsync after OTPs are verified by the controller.
    /// </summary>
    public async Task<Result> RegisterAsync(RegisterDto dto)
    {
        var byEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (byEmail is not null)
            return Result.Failure("A user with this email already exists.");

        // Phone uniqueness check (we treat it like a credential)
        var byPhone = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == dto.PhoneNumber);
        if (byPhone is not null)
            return Result.Failure("A user with this mobile number already exists.");

        return Result.Success();
    }

    public async Task<Result<ApplicationUser>> CompleteRegistrationAsync(RegisterDto dto)
    {
        // Re-check uniqueness (race) before insert
        var pre = await RegisterAsync(dto);
        if (!pre.IsSuccess) return Result<ApplicationUser>.Failure(pre.Error!);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            FullName = dto.FullName,
            City = dto.City,
            EmailConfirmed = true,           // verified just now via OTP
            PhoneNumberConfirmed = true,     // verified just now via OTP
            ShopName = dto.IsBarber ? dto.ShopName : null,
            ShopDescription = dto.IsBarber ? dto.ShopDescription : null,
            Address = dto.Address,
            OpeningTime = dto.IsBarber ? new TimeOnly(9, 0) : null,
            ClosingTime = dto.IsBarber ? new TimeOnly(20, 0) : null,
            WeeklyHoliday = dto.IsBarber ? DayOfWeek.Sunday : null,
            ApprovalStatus = dto.IsBarber ? BarberApprovalStatus.Pending : BarberApprovalStatus.NotApplicable,
            ApprovalRequestedAt = dto.IsBarber ? DateTime.UtcNow : null
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return Result<ApplicationUser>.Failure(result.Errors.Select(e => e.Description).ToList());

        var role = dto.IsBarber ? AppRoles.Admin : AppRoles.User;
        await _userManager.AddToRoleAsync(user, role);

        return Result<ApplicationUser>.Success(user);
    }

    public async Task<Result<ApplicationUser>> LoginAsync(LoginDto dto)
    {
        // Allow login with email OR phone number
        ApplicationUser? user = null;
        if (dto.EmailOrPhone.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(dto.EmailOrPhone);
        }
        else
        {
            user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == dto.EmailOrPhone);
        }

        if (user is null)
            return Result<ApplicationUser>.Failure("Invalid credentials.");

        if (!user.IsActive)
            return Result<ApplicationUser>.Failure("Your account is deactivated.");

        // Always persistent — user stays logged in across browser restarts.
        // (The "Remember me" checkbox has been removed from the login UI.)
        var signInResult = await _signInManager.PasswordSignInAsync(
            user.UserName!, dto.Password, isPersistent: true, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
                return Result<ApplicationUser>.Failure("Account locked. Try again later.");
            return Result<ApplicationUser>.Failure("Invalid credentials.");
        }

        return Result<ApplicationUser>.Success(user);
    }

    public Task LogoutAsync() => _signInManager.SignOutAsync();

    public async Task<Result> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Success(); // Don't reveal existence

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var baseUrl = _configuration["AppSettings:BaseUrl"];
        var resetLink = $"{baseUrl}/Auth/ResetPassword?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

        var body = $@"
            <h2>Reset Your Password</h2>
            <p>Hi {user.FullName},</p>
            <p>Click the link below to reset your password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you did not request this, ignore this email.</p>";

        await _emailService.SendAsync(email, "Reset Your Password - Barber Hub", body);

        // For demo: log reset link so dev can use it without real email
        _logger.LogInformation("Password reset link for {Email}: {Link}", email, resetLink);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Result.Failure("Invalid request.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<Result> UpdateProfileAsync(string userId, EditProfileDto dto, string? imagePath)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure("User not found.");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        user.City = dto.City;
        user.Address = dto.Address;
        user.Latitude = dto.Latitude;
        user.Longitude = dto.Longitude;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(imagePath))
            user.ProfileImageUrl = imagePath;

        // Barber fields
        if (await _userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            user.ShopName = dto.ShopName;
            user.ShopDescription = dto.ShopDescription;
            user.WeeklyHoliday = dto.WeeklyHoliday;
            user.OpeningTime = dto.OpeningTime;
            user.ClosingTime = dto.ClosingTime;
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    public Task<ApplicationUser?> GetByIdAsync(string userId) => _userManager.FindByIdAsync(userId);
}
