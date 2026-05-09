namespace BarberHub.Web.Domain.Constants;

public static class AppRoles
{
    public const string User = "User";
    public const string Admin = "Admin";              // Admin = Barber
    public const string SuperAdmin = "SuperAdmin";    // Platform owner

    public static readonly string[] All = { User, Admin, SuperAdmin };
}
