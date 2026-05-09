using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Bookings.Dtos;
using BarberHub.Web.Features.Bookings.Services;
using BarberHub.Web.Features.Notifications;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Filters;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Bookings.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly IBookingService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly ApplicationDbContext _db;

    public BookingsController(
        IBookingService service,
        ICurrentUserService currentUser,
        INotificationService notifications,
        ApplicationDbContext db)
    {
        _service = service;
        _currentUser = currentUser;
        _notifications = notifications;
        _db = db;
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.User)]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill all required fields.";
            return RedirectToAction("Details", "Barbers", new { id = dto.BarberId });
        }

        var result = await _service.CreateAsync(_currentUser.UserId!, dto);
        if (!result.IsSuccess)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction("Details", "Barbers", new { id = dto.BarberId });
        }

        await _notifications.NotifyAsync(
            dto.BarberId,
            NotificationType.BookingCreated,
            "New booking request",
            $"You have a new booking request for {dto.AppointmentDate:MMM dd} at {dto.StartTime:HH:mm}.",
            "/Bookings/Manage");

        TempData["Success"] = "Booking created successfully. Awaiting barber confirmation.";
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.User)]
    public async Task<IActionResult> MyBookings()
    {
        var bookings = await _service.GetUserBookingsAsync(_currentUser.UserId!);
        return View(bookings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.User)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var booking = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        var result = await _service.CancelAsync(_currentUser.UserId!, id);
        if (result.IsSuccess && booking is not null)
        {
            await _notifications.NotifyAsync(
                booking.BarberId,
                NotificationType.BookingCancelled,
                "Booking cancelled",
                $"A booking for {booking.AppointmentDate:MMM dd} at {booking.StartTime:HH:mm} was cancelled.",
                "/Bookings/Manage");
        }
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Booking cancelled." : result.Error;
        return RedirectToAction(nameof(MyBookings));
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    [RequireApprovedBarber]
    public async Task<IActionResult> Manage()
    {
        var bookings = await _service.GetBarberBookingsAsync(_currentUser.UserId!);
        return View(bookings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    [RequireApprovedBarber]
    public async Task<IActionResult> UpdateStatus(UpdateBookingStatusDto dto)
    {
        var result = await _service.UpdateStatusAsync(_currentUser.UserId!, dto);
        if (result.IsSuccess)
        {
            var b = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.BookingId);
            if (b is not null)
            {
                var (type, title, message) = dto.NewStatus switch
                {
                    BookingStatus.Accepted => (NotificationType.BookingAccepted, "Booking accepted",
                        $"Your booking on {b.AppointmentDate:MMM dd} at {b.StartTime:HH:mm} has been accepted."),
                    BookingStatus.Rejected => (NotificationType.BookingRejected, "Booking rejected",
                        $"Your booking on {b.AppointmentDate:MMM dd} was rejected. {dto.RejectionReason ?? ""}".Trim()),
                    BookingStatus.Completed => (NotificationType.BookingCompleted, "Service completed",
                        $"Your appointment on {b.AppointmentDate:MMM dd} is marked complete. Hope you loved it!"),
                    _ => (NotificationType.BookingCreated, "Booking updated", "Your booking status was updated.")
                };
                await _notifications.NotifyAsync(b.UserId, type, title, message, "/Bookings/MyBookings");
            }
        }
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Booking updated." : result.Error;
        return RedirectToAction(nameof(Manage));
    }
}
