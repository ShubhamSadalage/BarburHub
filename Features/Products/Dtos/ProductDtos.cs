using System.ComponentModel.DataAnnotations;

namespace BarberHub.Web.Features.Products.Dtos;

public class ProductListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal EffectivePrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Category { get; set; } = string.Empty;
    public string BarberName { get; set; } = string.Empty;
    public string BarberId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateProductDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required, Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(0, 100)]
    public decimal? DiscountPercentage { get; set; }

    [Required, Range(0, 100000)]
    public int StockQuantity { get; set; }

    [Required, StringLength(100)]
    public string Category { get; set; } = "General";

    public IFormFile? Image { get; set; }
}

public class EditProductDto : CreateProductDto
{
    public Guid Id { get; set; }
    public string? ExistingImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
