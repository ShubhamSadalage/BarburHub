namespace BarberHub.Web.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;

    public string ReceiverId { get; set; } = string.Empty;
    public ApplicationUser Receiver { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
