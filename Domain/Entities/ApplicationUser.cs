using Microsoft.AspNetCore.Identity;

namespace BarberHub.Web.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }

    // Last known location — populated from browser geolocation, used for "nearby barbers"
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LocationUpdatedAt { get; set; }
    public string? LocationLabel { get; set; }   // human-readable, e.g. "Pune, MH"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Soft delete — preserves bookings/orders/chat history for audit
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }

    // External login: lets us tie a Google account back to this row
    public string? GoogleSubjectId { get; set; }

    // Barber-specific fields
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public DayOfWeek? WeeklyHoliday { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }

    // Barber approval workflow
    public BarberApprovalStatus ApprovalStatus { get; set; } = BarberApprovalStatus.NotApplicable;
    public DateTime? ApprovalRequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }
    public string? ApprovalRejectionReason { get; set; }

    // Navigation properties
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Booking> BarberBookings { get; set; } = new List<Booking>();
    public ICollection<Booking> UserBookings { get; set; } = new List<Booking>();
    public ICollection<LeaveDay> LeaveDays { get; set; } = new List<LeaveDay>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
