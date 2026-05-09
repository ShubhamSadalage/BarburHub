namespace BarberHub.Web.Domain.Entities;

/// <summary>
/// Records sensitive admin / SuperAdmin actions for accountability.
/// Required for any Play Store privacy-policy compliance and dispute resolution.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ActorUserId { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;        // e.g. "BarberApprove", "UserDelete"
    public string? TargetType { get; set; }                   // e.g. "ApplicationUser", "Product"
    public string? TargetId { get; set; }
    public string? Details { get; set; }                      // JSON or text
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
