using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Notifications;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BarberHub.Web.Features.Bookings.Services;

/// <summary>
/// Periodic worker that:
/// 1. Recomputes per-barber queue position for every Accepted booking happening today.
///    When position drops to a new lower number, sends an in-app notification:
///    "2 ahead of you", "1 ahead of you", "Your turn has come".
/// 2. Sends a reminder ~60 minutes before each Accepted appointment.
/// 3. Auto-cancels Pending bookings older than the configured threshold to keep
///    the queue clean.
/// </summary>
public class BookingQueueWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly QueueOptions _options;
    private readonly ILogger<BookingQueueWorker> _logger;

    public BookingQueueWorker(
        IServiceProvider services,
        IOptions<QueueOptions> options,
        ILogger<BookingQueueWorker> logger)
    {
        _services = services;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Slight initial delay so app finishes startup
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        var period = TimeSpan.FromSeconds(Math.Max(15, _options.TickIntervalSeconds));
        using var timer = new PeriodicTimer(period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await TickOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BookingQueueWorker tick failed");
            }
        }
    }

    private async Task TickOnceAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notif = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var nowLocal = DateTime.Now;
        var nowTime = TimeOnly.FromDateTime(nowLocal);

        // ---------- 1. Auto-cancel stale Pending bookings ----------
        var staleCutoff = DateTime.UtcNow.AddMinutes(-_options.AutoCancelPendingMinutes);
        var stale = await db.Bookings
            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt < staleCutoff)
            .ToListAsync(ct);

        foreach (var b in stale)
        {
            b.Status = BookingStatus.Cancelled;
            b.UpdatedAt = DateTime.UtcNow;
            // Notify both sides
            await notif.NotifyAsync(b.UserId, NotificationType.BookingCancelled,
                "Booking auto-cancelled",
                $"Your pending booking on {b.AppointmentDate:MMM dd} at {b.StartTime:HH:mm} was cancelled (no response).",
                "/Bookings/MyBookings");
            await notif.NotifyAsync(b.BarberId, NotificationType.BookingCancelled,
                "Pending booking expired",
                $"A pending booking on {b.AppointmentDate:MMM dd} at {b.StartTime:HH:mm} was auto-cancelled.",
                "/Bookings/Manage");
        }
        if (stale.Count > 0) await db.SaveChangesAsync(ct);

        // ---------- 2. Queue updates for today ----------
        // Pull all Accepted bookings for today, ordered by start time per barber
        var accepted = await db.Bookings
            .Where(b => b.Status == BookingStatus.Accepted && b.AppointmentDate == today)
            .OrderBy(b => b.BarberId).ThenBy(b => b.StartTime)
            .ToListAsync(ct);

        var byBarber = accepted.GroupBy(b => b.BarberId);
        bool anyChange = false;

        foreach (var grp in byBarber)
        {
            // Filter out bookings that have already finished
            var upcoming = grp.Where(b => b.EndTime > nowTime).ToList();
            for (int i = 0; i < upcoming.Count; i++)
            {
                var b = upcoming[i];
                int position = i; // 0 = your turn, 1 = next, ...

                if (b.LastQueuePositionNotified == position) continue;

                // Only notify if the new position is *lower* (queue moved forward).
                // First-time notify (LastQueuePositionNotified == null) is also OK,
                // but we suppress when the booking is far in the future (>= 6 ahead)
                // to avoid noise.
                if (b.LastQueuePositionNotified is null && position >= 6)
                {
                    b.LastQueuePositionNotified = position;
                    anyChange = true;
                    continue;
                }
                if (b.LastQueuePositionNotified.HasValue && position >= b.LastQueuePositionNotified.Value)
                    continue;

                if (position == 0)
                {
                    await notif.NotifyAsync(b.UserId, NotificationType.QueueYourTurn,
                        "Your turn has come!",
                        "Please head to the shop now — the barber is ready for you.",
                        "/Bookings/MyBookings");
                }
                else
                {
                    await notif.NotifyAsync(b.UserId, NotificationType.QueueAhead,
                        position == 1 ? "1 ahead of you" : $"{position} ahead of you",
                        $"Your appointment on {b.AppointmentDate:MMM dd} at {b.StartTime:HH:mm}.",
                        "/Bookings/MyBookings");
                }

                b.LastQueuePositionNotified = position;
                anyChange = true;
            }
        }

        if (anyChange) await db.SaveChangesAsync(ct);

        // ---------- 3. Reminders ~ReminderMinutesBefore mins before appointment ----------
        var remindWindow = TimeSpan.FromMinutes(_options.ReminderMinutesBefore);
        var slack = TimeSpan.FromSeconds(Math.Max(15, _options.TickIntervalSeconds));

        var future = await db.Bookings
            .Where(b => b.Status == BookingStatus.Accepted
                        && b.AppointmentDate == today)
            .ToListAsync(ct);

        foreach (var b in future)
        {
            var apptTime = b.AppointmentDate.ToDateTime(b.StartTime);
            var deltaUntilAppt = apptTime - nowLocal;

            // Fire if we're within [reminderMin, reminderMin - tick) before appointment
            if (deltaUntilAppt <= remindWindow && deltaUntilAppt > remindWindow - slack)
            {
                // Best-effort dedup: store reminder send via notification log isn't here,
                // but the natural slack window means at most 1 fire per appointment.
                await notif.NotifyAsync(b.UserId, NotificationType.BookingReminder,
                    $"Appointment in {(int)deltaUntilAppt.TotalMinutes} min",
                    $"Reminder: your appointment is at {b.StartTime:HH:mm} today.",
                    "/Bookings/MyBookings");
            }
        }
    }
}
