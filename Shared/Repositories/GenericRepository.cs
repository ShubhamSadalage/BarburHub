using System.Linq.Expressions;
using BarberHub.Web.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Shared.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

    public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

    public virtual void Update(T entity) => _dbSet.Update(entity);

    public virtual void Remove(T entity) => _dbSet.Remove(entity);

    public virtual async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
