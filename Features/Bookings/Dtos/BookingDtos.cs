using System.ComponentModel.DataAnnotations;
using BarberHub.Web.Domain.Entities;

namespace BarberHub.Web.Features.Bookings.Dtos;

public class CreateBookingDto
{
    [Required]
    public string BarberId { get; set; } = string.Empty;

    [Required]
    public Guid ServiceId { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    public TimeOnly StartTime { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class BookingListItemDto
{
    public Guid Id { get; set; }
    public string BarberId { get; set; } = string.Empty;
    public string BarberName { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateBookingStatusDto
{
    [Required]
    public Guid BookingId { get; set; }

    [Required]
    public BookingStatus NewStatus { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }
}
