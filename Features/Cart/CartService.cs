using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Cart;

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public int StockQuantity { get; set; }
}

public class CartSummaryDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(i => i.LineTotal);
    public decimal Tax => Math.Round(SubTotal * 0.18m, 2); // 18% GST
    public decimal Total => SubTotal + Tax;
    public int ItemCount => Items.Sum(i => i.Quantity);
}

public interface ICartService
{
    Task<CartSummaryDto> GetAsync(string userId);
    Task<Result> AddAsync(string userId, Guid productId, int quantity);
    Task<Result> UpdateQuantityAsync(string userId, Guid cartItemId, int quantity);
    Task<Result> RemoveAsync(string userId, Guid cartItemId);
    Task ClearAsync(string userId);
}

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartSummaryDto> GetAsync(string userId)
    {
        var items = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return new CartSummaryDto
        {
            Items = items.Select(c => new CartItemDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                ProductName = c.Product.Name,
                ImageUrl = c.Product.ImageUrl,
                UnitPrice = c.Product.EffectivePrice,
                Quantity = c.Quantity,
                StockQuantity = c.Product.StockQuantity
            }).ToList()
        };
    }

    public async Task<Result> AddAsync(string userId, Guid productId, int quantity)
    {
        if (quantity < 1) quantity = 1;
        var product = await _context.Products.FindAsync(productId);
        if (product is null || !product.IsActive) return Result.Failure("Product unavailable.");
        if (product.StockQuantity < quantity) return Result.Failure("Not enough stock.");

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existing is null)
        {
            await _context.CartItems.AddAsync(new CartItem
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            });
        }
        else
        {
            existing.Quantity += quantity;
            if (existing.Quantity > product.StockQuantity)
                existing.Quantity = product.StockQuantity;
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> UpdateQuantityAsync(string userId, Guid cartItemId, int quantity)
    {
        var item = await _context.CartItems
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (item is null) return Result.Failure("Cart item not found.");
        if (quantity < 1)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            if (quantity > item.Product.StockQuantity)
                return Result.Failure("Not enough stock.");
            item.Quantity = quantity;
        }
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(string userId, Guid cartItemId)
    {
        var item = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);
        if (item is null) return Result.Failure("Cart item not found.");

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task ClearAsync(string userId)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
