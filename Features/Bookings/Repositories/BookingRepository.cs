using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Bookings.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<List<Booking>> GetByUserAsync(string userId);
    Task<List<Booking>> GetByBarberAsync(string barberId);
    Task<bool> HasConflictAsync(string barberId, DateOnly date, TimeOnly start, TimeOnly end);
    Task AddAsync(Booking booking);
    void Update(Booking booking);
    Task<int> SaveChangesAsync();
}

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _context;

    public BookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Barber)
            .Include(b => b.Service)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetByUserAsync(string userId)
    {
        return await _context.Bookings
            .Include(b => b.Barber)
            .Include(b => b.Service)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.AppointmentDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByBarberAsync(string barberId)
    {
        return await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Service)
            .Where(b => b.BarberId == barberId)
            .OrderByDescending(b => b.AppointmentDate)
            .ThenByDescending(b => b.StartTime)
            .ToListAsync();
    }

    public async Task<bool> HasConflictAsync(string barberId, DateOnly date, TimeOnly start, TimeOnly end)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.BarberId == barberId &&
            b.AppointmentDate == date &&
            (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Accepted) &&
            ((start >= b.StartTime && start < b.EndTime) ||
             (end > b.StartTime && end <= b.EndTime) ||
             (start <= b.StartTime && end >= b.EndTime)));
    }

    public async Task AddAsync(Booking booking) => await _context.Bookings.AddAsync(booking);
    public void Update(Booking booking) => _context.Bookings.Update(booking);
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
