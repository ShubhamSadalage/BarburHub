using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Notifications;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Filters;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BarberHub.Web.Features.Admin;

// =================== DTOs ===================

public class UserListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class LeaveDayDto
{
    public Guid Id { get; set; }
    [Required] public DateOnly LeaveDate { get; set; }
    [StringLength(300)] public string? Reason { get; set; }
}

public class WeeklyHolidayDto
{
    [Required] public DayOfWeek WeeklyHoliday { get; set; }
}

public class AdminAddUserDto
{
    [Required, StringLength(120)] [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(20)] [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(100)] public string? City { get; set; }

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "Welcome@123";
}

public class AdminCreateBookingDto
{
    [Required] [Display(Name = "Customer")]
    public string UserId { get; set; } = string.Empty;

    [Required] [Display(Name = "Service")]
    public Guid ServiceId { get; set; }

    [Required] [Display(Name = "Date")]
    public DateOnly AppointmentDate { get; set; }

    [Required] [Display(Name = "Start time")]
    public TimeOnly StartTime { get; set; }

    [StringLength(500)] public string? Notes { get; set; }
}

public class EarningsDto
{
    public decimal TotalEarnings { get; set; }
    public decimal TodayEarnings { get; set; }
    public decimal ThisWeekEarnings { get; set; }
    public decimal ThisMonthEarnings { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalAccepted { get; set; }
    public List<DailyEarningRow> DailyBreakdown { get; set; } = new();
    public List<ServiceEarningRow> ByService { get; set; } = new();
}

public class DailyEarningRow
{
    public DateOnly Date { get; set; }
    public int BookingsCount { get; set; }
    public decimal Amount { get; set; }
}

public class ServiceEarningRow
{
    public string ServiceName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

// =================== Controller ===================

[Authorize(Roles = AppRoles.Admin)]
[RequireApprovedBarber]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        INotificationService notifications)
    {
        _context = context;
        _userManager = userManager;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    // ----------- Dashboard -----------
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var barberId = _currentUser.UserId!;
        ViewBag.BookingsCount = await _context.Bookings.CountAsync(b => b.BarberId == barberId);
        ViewBag.ProductsCount = await _context.Products.CountAsync(p => p.BarberId == barberId);
        ViewBag.PendingBookings = await _context.Bookings.CountAsync(b => b.BarberId == barberId && b.Status == BookingStatus.Pending);
        ViewBag.TodayBookings = await _context.Bookings.CountAsync(b => b.BarberId == barberId && b.AppointmentDate == DateOnly.FromDateTime(DateTime.Today));

        ViewBag.TotalEarnings = await _context.Bookings
            .Where(b => b.BarberId == barberId &&
                        (b.Status == BookingStatus.Completed || b.Status == BookingStatus.Accepted))
            .SumAsync(b => (decimal?)b.TotalAmount) ?? 0m;

        return View();
    }

    // ----------- Users (existing) -----------
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(AppRoles.User);
        var list = usersInRole.Select(u => new UserListItemDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? "",
            PhoneNumber = u.PhoneNumber,
            City = u.City,
            CreatedAt = u.CreatedAt,
            IsActive = u.IsActive
        }).OrderByDescending(u => u.CreatedAt).ToList();
        return View(list);
    }

    // ----------- Add User (NEW) -----------
    [HttpGet]
    public IActionResult AddUser() => View(new AdminAddUserDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddUser(AdminAddUserDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(dto.Email), "A user with this email already exists.");
            return View(dto);
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            PhoneNumberConfirmed = !string.IsNullOrEmpty(dto.PhoneNumber),
            City = dto.City,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(dto);
        }

        await _userManager.AddToRoleAsync(user, AppRoles.User);
        TempData["Success"] = $"Customer '{user.FullName}' added successfully.";
        return RedirectToAction(nameof(Users));
    }

    // ----------- Create Booking on behalf of any user (NEW) -----------
    [HttpGet]
    public async Task<IActionResult> CreateBooking()
    {
        var barberId = _currentUser.UserId!;
        await PopulateCreateBookingViewBag(barberId);
        return View(new AdminCreateBookingDto { AppointmentDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBooking(AdminCreateBookingDto dto)
    {
        var barberId = _currentUser.UserId!;
        if (!ModelState.IsValid)
        {
            await PopulateCreateBookingViewBag(barberId);
            return View(dto);
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.BarberId == barberId && s.IsActive);
        if (service is null)
        {
            TempData["Error"] = "Selected service not found.";
            await PopulateCreateBookingViewBag(barberId);
            return View(dto);
        }

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
        {
            TempData["Error"] = "Selected customer not found.";
            await PopulateCreateBookingViewBag(barberId);
            return View(dto);
        }

        var endTime = dto.StartTime.AddMinutes(service.DurationMinutes);

        var conflict = await _context.Bookings.AnyAsync(b =>
            b.BarberId == barberId &&
            b.AppointmentDate == dto.AppointmentDate &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.Rejected &&
            b.StartTime < endTime &&
            b.EndTime > dto.StartTime);

        if (conflict)
        {
            TempData["Error"] = "This time slot conflicts with an existing booking.";
            await PopulateCreateBookingViewBag(barberId);
            return View(dto);
        }

        var barber = await _userManager.FindByIdAsync(barberId);
        if (barber?.WeeklyHoliday is not null && (int)barber.WeeklyHoliday == (int)dto.AppointmentDate.DayOfWeek)
        {
            TempData["Error"] = "Selected date is your weekly holiday.";
            await PopulateCreateBookingViewBag(barberId);
            return View(dto);
        }
        var onLeave = await _context.LeaveDays.AnyAsync(l => l.BarberId == barberId && l.LeaveDate == dto.AppointmentDate);
        if (onLeave)
        {
            TempData["Error"] = "You have marked this date as a leave day.";
            await PopulateCreateBookingViewBag(barberId);
            return View(dto);
        }

        var booking = new Booking
        {
            UserId = dto.UserId,
            BarberId = barberId,
            ServiceId = service.Id,
            AppointmentDate = dto.AppointmentDate,
            StartTime = dto.StartTime,
            EndTime = endTime,
            Status = BookingStatus.Accepted,   // barber-created = auto-accepted
            Notes = dto.Notes,
            TotalAmount = service.Price,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();

        await _notifications.NotifyAsync(
            dto.UserId,
            NotificationType.BookingAccepted,
            "Booking confirmed",
            $"Your barber has booked you for {service.Name} on {dto.AppointmentDate:MMM dd} at {dto.StartTime:HH:mm}.",
            "/Bookings/MyBookings");

        TempData["Success"] = $"Booking confirmed for {user.FullName} on {dto.AppointmentDate:MMM dd} at {dto.StartTime:HH:mm}.";
        return RedirectToAction("Manage", "Bookings");
    }

    private async Task PopulateCreateBookingViewBag(string barberId)
    {
        var users = await _userManager.GetUsersInRoleAsync(AppRoles.User);
        ViewBag.Users = users.OrderBy(u => u.FullName)
            .Select(u => new SelectListItem { Value = u.Id, Text = $"{u.FullName} ({u.Email})" }).ToList();

        var services = await _context.Services
            .Where(s => s.BarberId == barberId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} — ₹{s.Price} ({s.DurationMinutes} min)"
            }).ToListAsync();
        ViewBag.Services = services;
    }

    // ----------- Earnings (NEW) -----------
    [HttpGet]
    public async Task<IActionResult> Earnings(DateOnly? from = null, DateOnly? to = null)
    {
        var barberId = _currentUser.UserId!;
        var rangeFrom = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-29));
        var rangeTo = to ?? DateOnly.FromDateTime(DateTime.Today);
        if (rangeTo < rangeFrom) (rangeFrom, rangeTo) = (rangeTo, rangeFrom);

        var earningStatuses = new[] { BookingStatus.Completed, BookingStatus.Accepted };
        var allEarning = await _context.Bookings
            .Where(b => b.BarberId == barberId && earningStatuses.Contains(b.Status))
            .Include(b => b.Service)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var startOfWeek = today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);

        var dto = new EarningsDto
        {
            TotalEarnings = allEarning.Sum(b => b.TotalAmount),
            TodayEarnings = allEarning.Where(b => b.AppointmentDate == today).Sum(b => b.TotalAmount),
            ThisWeekEarnings = allEarning.Where(b => b.AppointmentDate >= startOfWeek).Sum(b => b.TotalAmount),
            ThisMonthEarnings = allEarning.Where(b => b.AppointmentDate >= startOfMonth).Sum(b => b.TotalAmount),
            TotalCompleted = allEarning.Count(b => b.Status == BookingStatus.Completed),
            TotalAccepted = allEarning.Count(b => b.Status == BookingStatus.Accepted),

            DailyBreakdown = allEarning
                .Where(b => b.AppointmentDate >= rangeFrom && b.AppointmentDate <= rangeTo)
                .GroupBy(b => b.AppointmentDate)
                .Select(g => new DailyEarningRow
                {
                    Date = g.Key,
                    BookingsCount = g.Count(),
                    Amount = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(r => r.Date).ToList(),

            ByService = allEarning
                .Where(b => b.AppointmentDate >= rangeFrom && b.AppointmentDate <= rangeTo)
                .GroupBy(b => b.Service.Name)
                .Select(g => new ServiceEarningRow
                {
                    ServiceName = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(b => b.TotalAmount)
                })
                .OrderByDescending(r => r.Amount).ToList()
        };

        ViewBag.From = rangeFrom;
        ViewBag.To = rangeTo;
        return View(dto);
    }

    // ----------- LeaveDays (existing, unchanged) -----------
    [HttpGet]
    public async Task<IActionResult> LeaveDays()
    {
        var barberId = _currentUser.UserId!;
        var leaves = await _context.LeaveDays
            .Where(l => l.BarberId == barberId && l.LeaveDate >= DateOnly.FromDateTime(DateTime.Today.AddDays(-30)))
            .OrderBy(l => l.LeaveDate).ToListAsync();

        var dtos = leaves.Select(l => new LeaveDayDto { Id = l.Id, LeaveDate = l.LeaveDate, Reason = l.Reason }).ToList();
        var barber = await _userManager.FindByIdAsync(barberId);
        ViewBag.WeeklyHoliday = barber?.WeeklyHoliday;
        return View(dtos);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLeaveDay(LeaveDayDto dto)
    {
        var barberId = _currentUser.UserId!;
        if (dto.LeaveDate < DateOnly.FromDateTime(DateTime.Today))
        {
            TempData["Error"] = "Cannot add leave in the past.";
            return RedirectToAction(nameof(LeaveDays));
        }

        var existing = await _context.LeaveDays.FirstOrDefaultAsync(l => l.BarberId == barberId && l.LeaveDate == dto.LeaveDate);
        if (existing is not null)
        {
            TempData["Error"] = "Leave already set for this date.";
            return RedirectToAction(nameof(LeaveDays));
        }

        await _context.LeaveDays.AddAsync(new LeaveDay
        {
            BarberId = barberId, LeaveDate = dto.LeaveDate, Reason = dto.Reason
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = "Leave day added.";
        return RedirectToAction(nameof(LeaveDays));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLeaveDay(Guid id)
    {
        var barberId = _currentUser.UserId!;
        var leave = await _context.LeaveDays.FirstOrDefaultAsync(l => l.Id == id && l.BarberId == barberId);
        if (leave is not null)
        {
            _context.LeaveDays.Remove(leave);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Leave removed.";
        }
        return RedirectToAction(nameof(LeaveDays));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetWeeklyHoliday(WeeklyHolidayDto dto)
    {
        var barber = await _userManager.FindByIdAsync(_currentUser.UserId!);
        if (barber is not null)
        {
            barber.WeeklyHoliday = dto.WeeklyHoliday;
            await _userManager.UpdateAsync(barber);
            TempData["Success"] = $"Weekly holiday set to {dto.WeeklyHoliday}.";
        }
        return RedirectToAction(nameof(LeaveDays));
    }
}
