using System.Text.Json;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;

namespace BarberHub.Web.Shared.Services;

public class PushPayload
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Tag { get; set; }
}

public interface IPushService
{
    Task<int> SendToUserAsync(string userId, PushPayload payload);
    Task SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth, string? userAgent);
    Task RemoveSubscriptionAsync(string endpoint);
    string? GetVapidPublicKey();
    bool IsConfigured();
}

public class PushService : IPushService
{
    private readonly ApplicationDbContext _db;
    private readonly WebPushOptions _opts;
    private readonly FeatureFlags _features;
    private readonly ILogger<PushService> _logger;

    public PushService(
        ApplicationDbContext db,
        IOptions<WebPushOptions> opts,
        IOptions<FeatureFlags> features,
        ILogger<PushService> logger)
    {
        _db = db;
        _opts = opts.Value;
        _features = features.Value;
        _logger = logger;
    }

    public bool IsConfigured() =>
        _features.WebPushEnabled
        && !string.IsNullOrEmpty(_opts.VapidPublicKey)
        && !string.IsNullOrEmpty(_opts.VapidPrivateKey);

    public string? GetVapidPublicKey() => IsConfigured() ? _opts.VapidPublicKey : null;

    public async Task<int> SendToUserAsync(string userId, PushPayload payload)
    {
        if (!IsConfigured()) return 0;

        var subs = await _db.PushSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
        if (subs.Count == 0) return 0;

        var client = new WebPushClient();
        var vapid = new VapidDetails(_opts.VapidSubject, _opts.VapidPublicKey, _opts.VapidPrivateKey);
        var body = JsonSerializer.Serialize(new
        {
            title = payload.Title,
            body = payload.Body,
            url = payload.Url ?? "/",
            tag = payload.Tag ?? Guid.NewGuid().ToString("N")
        });

        int sent = 0;
        foreach (var s in subs)
        {
            var sub = new WebPush.PushSubscription(s.Endpoint, s.P256dh, s.Auth);
            try
            {
                await client.SendNotificationAsync(sub, body, vapid);
                s.LastSeenAt = DateTime.UtcNow;
                sent++;
            }
            catch (WebPushException ex)
            {
                // 404 / 410 = endpoint gone — deactivate so we stop trying
                if ((int)ex.StatusCode == 404 || (int)ex.StatusCode == 410)
                {
                    s.IsActive = false;
                    _logger.LogInformation("Deactivated dead push subscription {Endpoint}", s.Endpoint);
                }
                else
                {
                    _logger.LogWarning(ex, "Push send failed for {Endpoint}", s.Endpoint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push send unexpected error");
            }
        }
        await _db.SaveChangesAsync();
        return sent;
    }

    public async Task SaveSubscriptionAsync(string userId, string endpoint, string p256dh, string auth, string? userAgent)
    {
        var existing = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing is not null)
        {
            existing.UserId = userId;
            existing.P256dh = p256dh;
            existing.Auth = auth;
            existing.UserAgent = userAgent;
            existing.LastSeenAt = DateTime.UtcNow;
            existing.IsActive = true;
        }
        else
        {
            await _db.PushSubscriptions.AddAsync(new Domain.Entities.PushSubscription
            {
                UserId = userId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth,
                UserAgent = userAgent
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task RemoveSubscriptionAsync(string endpoint)
    {
        var sub = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (sub is null) return;
        sub.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
