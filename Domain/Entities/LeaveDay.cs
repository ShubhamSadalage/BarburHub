namespace BarberHub.Web.Domain.Entities;

public class LeaveDay
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string BarberId { get; set; } = string.Empty;
    public ApplicationUser Barber { get; set; } = null!;

    public DateOnly LeaveDate { get; set; }
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
