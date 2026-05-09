namespace BarberHub.Web.Domain.Entities;

public enum BookingStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4
}

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string BarberId { get; set; } = string.Empty;
    public ApplicationUser Barber { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Last queue-position number we sent a notification for (so we don't spam:
    /// we send when this changes from 5→4→3→2→1→0). 0 means "your turn".
    /// Null means we haven't sent any queue notification yet.
    /// </summary>
    public int? LastQueuePositionNotified { get; set; }
}
