using System.Security.Claims;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;

namespace BarberHub.Web.Shared.Services;

public interface IAuditService
{
    Task LogAsync(ClaimsPrincipal actor, string action, string? targetType = null, string? targetId = null, string? details = null, string? ip = null);
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db) { _db = db; }

    public async Task LogAsync(ClaimsPrincipal actor, string action, string? targetType = null, string? targetId = null, string? details = null, string? ip = null)
    {
        var entry = new AuditLog
        {
            ActorUserId = actor.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown",
            ActorEmail = actor.FindFirstValue(ClaimTypes.Email) ?? actor.Identity?.Name ?? "unknown",
            ActorRole = actor.FindFirst(ClaimTypes.Role)?.Value ?? string.Join(",", actor.FindAll(ClaimTypes.Role).Select(c => c.Value)),
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            IpAddress = ip
        };
        await _db.AuditLogs.AddAsync(entry);
        await _db.SaveChangesAsync();
    }
}
