using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Notifications;

// =================== DTOs ===================

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

// =================== Service ===================

public interface INotificationService
{
    Task NotifyAsync(string recipientUserId, NotificationType type, string title, string message, string? linkUrl = null);
    Task NotifyAllSuperAdminsAsync(NotificationType type, string title, string message, string? linkUrl = null);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly Shared.Services.IPushService _push;

    public NotificationService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IHubContext<NotificationHub> hub,
        Shared.Services.IPushService push)
    {
        _db = db; _userManager = userManager; _hub = hub; _push = push;
    }

    public async Task NotifyAsync(string recipientUserId, NotificationType type, string title, string message, string? linkUrl = null)
    {
        var n = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl
        };
        await _db.Notifications.AddAsync(n);
        await _db.SaveChangesAsync();

        // 1) SignalR — live update for users with the page open
        await _hub.Clients.User(recipientUserId).SendAsync("ReceiveNotification", new NotificationDto
        {
            Id = n.Id, Type = n.Type, Title = n.Title, Message = n.Message,
            LinkUrl = n.LinkUrl, IsRead = false, CreatedAt = n.CreatedAt
        });

        // 2) Web Push — works in TWA background, lock screen, app closed
        try
        {
            await _push.SendToUserAsync(recipientUserId, new Shared.Services.PushPayload
            {
                Title = title, Body = message, Url = linkUrl, Tag = type.ToString()
            });
        }
        catch
        {
            // Push failure must never break in-app notifications
        }
    }

    public async Task NotifyAllSuperAdminsAsync(NotificationType type, string title, string message, string? linkUrl = null)
    {
        var superAdmins = await _userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin);
        foreach (var sa in superAdmins)
            await NotifyAsync(sa.Id, type, title, message, linkUrl);
    }
}

// =================== SignalR Hub ===================

[Authorize]
public class NotificationHub : Hub
{
    // SignalR auto-routes Clients.User(userId) to all connections of that user.
    // No additional logic needed here; just authenticate.
}

// =================== Controller (bell icon endpoints) ===================

[Authorize]
[Route("[controller]/[action]")]
public class NotificationsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db; _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Recent()
    {
        var userId = _currentUser.UserId!;
        // Show only UNREAD notifications in the bell — read ones disappear from the dropdown
        var items = await _db.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new NotificationDto
            {
                Id = n.Id, Type = n.Type, Title = n.Title, Message = n.Message,
                LinkUrl = n.LinkUrl, IsRead = n.IsRead, CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        var unreadCount = items.Count;
        return Json(new { items, unreadCount });
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = _currentUser.UserId!;
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.RecipientUserId == userId);
        if (n is not null && !n.IsRead)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUser.UserId!;
        var unread = await _db.Notifications.Where(n => n.RecipientUserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }
}
