using System.ComponentModel.DataAnnotations;

namespace BarberHub.Web.Features.Auth.Dtos;

public class RegisterDto
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? City { get; set; }

    [Display(Name = "Register as Barber")]
    public bool IsBarber { get; set; } = false;

    // Barber-only fields
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public string? Address { get; set; }
}

public class RegisterVerifyDto
{
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(8, MinimumLength = 4)]
    [Display(Name = "Email code")]
    public string EmailCode { get; set; } = string.Empty;

    [Required, StringLength(8, MinimumLength = 4)]
    [Display(Name = "Mobile code")]
    public string MobileCode { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, Display(Name = "Email or Phone")]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required, DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare("NewPassword")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class EditProfileDto
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? City { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public IFormFile? ProfileImage { get; set; }

    // Barber fields
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public DayOfWeek? WeeklyHoliday { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
}
