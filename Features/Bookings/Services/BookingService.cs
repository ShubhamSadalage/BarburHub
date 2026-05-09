using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Bookings.Dtos;
using BarberHub.Web.Features.Bookings.Repositories;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Bookings.Services;

public interface IBookingService
{
    Task<Result<Guid>> CreateAsync(string userId, CreateBookingDto dto);
    Task<List<BookingListItemDto>> GetUserBookingsAsync(string userId);
    Task<List<BookingListItemDto>> GetBarberBookingsAsync(string barberId);
    Task<Result> UpdateStatusAsync(string barberId, UpdateBookingStatusDto dto);
    Task<Result> CancelAsync(string userId, Guid bookingId);
}

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repo;
    private readonly ApplicationDbContext _context;

    public BookingService(IBookingRepository repo, ApplicationDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    public async Task<Result<Guid>> CreateAsync(string userId, CreateBookingDto dto)
    {
        var service = await _context.Services
            .Include(s => s.Barber)
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.BarberId == dto.BarberId);

        if (service is null)
            return Result<Guid>.Failure("Service not found.");

        if (dto.AppointmentDate < DateOnly.FromDateTime(DateTime.Today))
            return Result<Guid>.Failure("Cannot book appointments in the past.");

        var endTime = dto.StartTime.AddMinutes(service.DurationMinutes);

        // Weekly holiday check
        if (service.Barber.WeeklyHoliday.HasValue && dto.AppointmentDate.DayOfWeek == service.Barber.WeeklyHoliday.Value)
            return Result<Guid>.Failure("Barber is closed on this day.");

        // Leave day check
        var isLeave = await _context.LeaveDays.AnyAsync(l => l.BarberId == dto.BarberId && l.LeaveDate == dto.AppointmentDate);
        if (isLeave)
            return Result<Guid>.Failure("Barber is on leave on this day.");

        if (await _repo.HasConflictAsync(dto.BarberId, dto.AppointmentDate, dto.StartTime, endTime))
            return Result<Guid>.Failure("This time slot is no longer available.");

        var booking = new Booking
        {
            UserId = userId,
            BarberId = dto.BarberId,
            ServiceId = dto.ServiceId,
            AppointmentDate = dto.AppointmentDate,
            StartTime = dto.StartTime,
            EndTime = endTime,
            TotalAmount = service.Price,
            Notes = dto.Notes,
            Status = BookingStatus.Pending
        };

        await _repo.AddAsync(booking);
        await _repo.SaveChangesAsync();

        return Result<Guid>.Success(booking.Id);
    }

    public async Task<List<BookingListItemDto>> GetUserBookingsAsync(string userId)
    {
        var list = await _repo.GetByUserAsync(userId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<BookingListItemDto>> GetBarberBookingsAsync(string barberId)
    {
        var list = await _repo.GetByBarberAsync(barberId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<Result> UpdateStatusAsync(string barberId, UpdateBookingStatusDto dto)
    {
        var booking = await _repo.GetByIdAsync(dto.BookingId);
        if (booking is null) return Result.Failure("Booking not found.");
        if (booking.BarberId != barberId) return Result.Failure("Not authorized.");

        booking.Status = dto.NewStatus;
        booking.RejectionReason = dto.RejectionReason;
        booking.UpdatedAt = DateTime.UtcNow;

        _repo.Update(booking);
        await _repo.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CancelAsync(string userId, Guid bookingId)
    {
        var booking = await _repo.GetByIdAsync(bookingId);
        if (booking is null) return Result.Failure("Booking not found.");
        if (booking.UserId != userId) return Result.Failure("Not authorized.");

        if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
            return Result.Failure("Cannot cancel this booking.");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;
        _repo.Update(booking);
        await _repo.SaveChangesAsync();
        return Result.Success();
    }

    private static BookingListItemDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        BarberId = b.BarberId,
        BarberName = b.Barber?.FullName ?? "",
        ShopName = b.Barber?.ShopName ?? b.Barber?.FullName ?? "",
        UserId = b.UserId,
        UserName = b.User?.FullName ?? "",
        UserPhone = b.User?.PhoneNumber ?? "",
        ServiceName = b.Service?.Name ?? "",
        AppointmentDate = b.AppointmentDate,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        Status = b.Status,
        TotalAmount = b.TotalAmount,
        Notes = b.Notes,
        RejectionReason = b.RejectionReason,
        CreatedAt = b.CreatedAt
    };
}
