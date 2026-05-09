namespace BarberHub.Web.Domain.Entities;

/// <summary>
/// Browser/TWA Web Push subscription. One user can have multiple devices.
/// Endpoint + keys come from the browser's PushManager.subscribe() call.
/// </summary>
public class PushSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string Endpoint { get; set; } = string.Empty;       // unique per device
    public string P256dh { get; set; } = string.Empty;          // public key from PushSubscription.toJSON().keys.p256dh
    public string Auth { get; set; } = string.Empty;            // auth secret from PushSubscription.toJSON().keys.auth
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
