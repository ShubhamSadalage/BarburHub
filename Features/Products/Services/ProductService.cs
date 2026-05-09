using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Products.Dtos;
using BarberHub.Web.Features.Products.Repositories;
using BarberHub.Web.Shared;

namespace BarberHub.Web.Features.Products.Services;

public interface IProductService
{
    Task<List<ProductListItemDto>> GetAllActiveAsync();
    Task<List<ProductListItemDto>> GetByBarberAsync(string barberId);
    Task<ProductListItemDto?> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(string barberId, CreateProductDto dto, string? imagePath);
    Task<Result> UpdateAsync(string barberId, EditProductDto dto, string? imagePath);
    Task<Result> DeleteAsync(string barberId, Guid id);
    Task<EditProductDto?> GetForEditAsync(string barberId, Guid id);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ProductListItemDto>> GetAllActiveAsync() =>
        (await _repo.GetAllActiveAsync()).Select(Map).ToList();

    public async Task<List<ProductListItemDto>> GetByBarberAsync(string barberId) =>
        (await _repo.GetByBarberAsync(barberId)).Select(Map).ToList();

    public async Task<ProductListItemDto?> GetByIdAsync(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p is null ? null : Map(p);
    }

    public async Task<Result<Guid>> CreateAsync(string barberId, CreateProductDto dto, string? imagePath)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPercentage = dto.DiscountPercentage,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category,
            ImageUrl = imagePath,
            BarberId = barberId,
            IsActive = true
        };
        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();
        return Result<Guid>.Success(product.Id);
    }

    public async Task<Result> UpdateAsync(string barberId, EditProductDto dto, string? imagePath)
    {
        var product = await _repo.GetByIdAsync(dto.Id);
        if (product is null) return Result.Failure("Product not found.");
        if (product.BarberId != barberId) return Result.Failure("Not authorized.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.DiscountPercentage = dto.DiscountPercentage;
        product.StockQuantity = dto.StockQuantity;
        product.Category = dto.Category;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(imagePath)) product.ImageUrl = imagePath;

        _repo.Update(product);
        await _repo.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(string barberId, Guid id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return Result.Failure("Product not found.");
        if (product.BarberId != barberId) return Result.Failure("Not authorized.");

        _repo.Remove(product);
        await _repo.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<EditProductDto?> GetForEditAsync(string barberId, Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p is null || p.BarberId != barberId) return null;

        return new EditProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DiscountPercentage = p.DiscountPercentage,
            StockQuantity = p.StockQuantity,
            Category = p.Category,
            ExistingImageUrl = p.ImageUrl,
            IsActive = p.IsActive
        };
    }

    private static ProductListItemDto Map(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        DiscountPercentage = p.DiscountPercentage,
        EffectivePrice = p.EffectivePrice,
        StockQuantity = p.StockQuantity,
        ImageUrl = p.ImageUrl,
        Category = p.Category,
        BarberId = p.BarberId ?? string.Empty,
        BarberName = p.Barber?.ShopName ?? p.Barber?.FullName ?? "Marketplace",
        IsActive = p.IsActive
    };
}
