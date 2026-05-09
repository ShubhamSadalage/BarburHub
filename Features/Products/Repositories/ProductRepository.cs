using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Products.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetAllActiveAsync();
    Task<List<Product>> GetByBarberAsync(string barberId, bool onlyActive = false);
    Task AddAsync(Product product);
    void Update(Product product);
    void Remove(Product product);
    Task<int> SaveChangesAsync();
}

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await _context.Products.Include(p => p.Barber).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetAllActiveAsync() =>
        await _context.Products.Include(p => p.Barber).Where(p => p.IsActive && p.StockQuantity > 0).ToListAsync();

    public async Task<List<Product>> GetByBarberAsync(string barberId, bool onlyActive = false)
    {
        var query = _context.Products.Where(p => p.BarberId == barberId);
        if (onlyActive) query = query.Where(p => p.IsActive);
        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task AddAsync(Product product) => await _context.Products.AddAsync(product);
    public void Update(Product product) => _context.Products.Update(product);
    public void Remove(Product product) => _context.Products.Remove(product);
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
