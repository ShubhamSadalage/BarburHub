namespace BarberHub.Web.Domain.Entities;

public enum NotificationType
{
    BarberRegistration = 0,     // To SuperAdmin: new barber registered
    BarberApproved = 1,         // To barber: approval granted
    BarberRejected = 2,         // To barber: approval denied
    BookingCreated = 3,         // To barber: new booking
    BookingAccepted = 4,        // To user
    BookingRejected = 5,        // To user
    BookingCompleted = 6,       // To user
    BookingCancelled = 7,       // To barber
    OrderPlaced = 8,            // To barber/super-admin
    NewMessage = 9,             // Chat
    QueueAhead = 10,            // "2 ahead of you" / "1 ahead of you"
    QueueYourTurn = 11,         // "Your turn has come"
    PaymentReceived = 12,       // COD marked paid by seller
    BookingReminder = 13,       // ~1 hour before appointment
    UserDeleted = 14,           // For audit visibility
    SystemAnnouncement = 15
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RecipientUserId { get; set; } = string.Empty;
    public ApplicationUser Recipient { get; set; } = null!;

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
