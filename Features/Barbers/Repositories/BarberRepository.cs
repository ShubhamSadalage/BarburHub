using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Barbers.Repositories;

public interface IBarberRepository
{
    Task<List<ApplicationUser>> SearchAsync(string? city);
    Task<ApplicationUser?> GetBarberWithDetailsAsync(string barberId);
    Task<List<DateOnly>> GetLeaveDatesAsync(string barberId, DateOnly from, DateOnly to);
    Task<List<Booking>> GetBookingsForDateAsync(string barberId, DateOnly date);
    Task<List<Service>> GetServicesAsync(string barberId);
}

public class BarberRepository : IBarberRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BarberRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<ApplicationUser>> SearchAsync(string? city)
    {
        var barbers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        IEnumerable<ApplicationUser> query = barbers.Where(b =>
            b.IsActive
            && !b.IsDeleted
            && b.ApprovalStatus == BarberApprovalStatus.Approved);

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(b => b.City != null &&
                b.City.Contains(city, StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    public async Task<ApplicationUser?> GetBarberWithDetailsAsync(string barberId)
    {
        return await _context.Users
            .Include(u => u.Services.Where(s => s.IsActive))
            .FirstOrDefaultAsync(u => u.Id == barberId);
    }

    public async Task<List<DateOnly>> GetLeaveDatesAsync(string barberId, DateOnly from, DateOnly to)
    {
        return await _context.LeaveDays
            .Where(l => l.BarberId == barberId && l.LeaveDate >= from && l.LeaveDate <= to)
            .Select(l => l.LeaveDate)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsForDateAsync(string barberId, DateOnly date)
    {
        return await _context.Bookings
            .Where(b => b.BarberId == barberId
                     && b.AppointmentDate == date
                     && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Accepted))
            .ToListAsync();
    }

    public async Task<List<Service>> GetServicesAsync(string barberId)
    {
        return await _context.Services
            .Where(s => s.BarberId == barberId && s.IsActive)
            .ToListAsync();
    }
}
