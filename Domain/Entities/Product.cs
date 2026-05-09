namespace BarberHub.Web.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = "General";
    public bool IsActive { get; set; } = true;

    // BarberId == null  → marketplace product owned/managed by SuperAdmin
    // BarberId != null  → product owned by that specific barber
    public string? BarberId { get; set; }
    public ApplicationUser? Barber { get; set; }

    public bool IsMarketplace => string.IsNullOrEmpty(BarberId);

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public decimal EffectivePrice => DiscountPercentage.HasValue
        ? Price - (Price * DiscountPercentage.Value / 100m)
        : Price;
}
