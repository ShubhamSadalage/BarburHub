using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Barbers.Dtos;
using BarberHub.Web.Features.Barbers.Repositories;

namespace BarberHub.Web.Features.Barbers.Services;

public interface IBarberService
{
    Task<List<BarberListItemDto>> SearchAsync(BarberSearchDto dto);
    Task<BarberDetailsDto?> GetDetailsAsync(string barberId);
    Task<AvailableSlotsDto> GetAvailableSlotsAsync(string barberId, DateOnly date, Guid serviceId);

    /// <summary>
    /// Home-page barber listing with smart location fallback:
    /// 1. If lat/lng provided → nearby (sorted by distance, capped to 25km)
    /// 2. Else if user has saved location on profile → nearby from saved location
    /// 3. Else if user has a city → barbers in same city
    /// 4. Else → newest 12 approved barbers
    /// </summary>
    Task<HomeBarberListDto> GetForHomeAsync(string? userId, double? lat, double? lng, int limit = 12);
}

public class BarberService : IBarberService
{
    private readonly IBarberRepository _repo;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

    public BarberService(
        IBarberRepository repo,
        Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _userManager = userManager;
    }

    public async Task<HomeBarberListDto> GetForHomeAsync(string? userId, double? lat, double? lng, int limit = 12)
    {
        // Step 1: If client supplied live coordinates, use them
        if (lat.HasValue && lng.HasValue)
        {
            var list = await SearchAsync(new BarberSearchDto
            {
                Latitude = lat,
                Longitude = lng,
                RadiusKm = 25
            });
            if (list.Any())
                return new HomeBarberListDto { Source = HomeBarberSource.LiveLocation, Items = list.Take(limit).ToList() };
        }

        // Step 2: Use the user's previously stored location, if any
        if (!string.IsNullOrEmpty(userId))
        {
            var u = await _userManager.FindByIdAsync(userId);
            if (u is not null && u.Latitude.HasValue && u.Longitude.HasValue)
            {
                var list = await SearchAsync(new BarberSearchDto
                {
                    Latitude = u.Latitude,
                    Longitude = u.Longitude,
                    RadiusKm = 25
                });
                if (list.Any())
                    return new HomeBarberListDto
                    {
                        Source = HomeBarberSource.SavedLocation,
                        Label = u.LocationLabel ?? u.City,
                        Items = list.Take(limit).ToList()
                    };
            }

            // Step 3: City-based fallback
            if (u is not null && !string.IsNullOrWhiteSpace(u.City))
            {
                var list = await SearchAsync(new BarberSearchDto { City = u.City, RadiusKm = 9999 });
                if (list.Any())
                    return new HomeBarberListDto
                    {
                        Source = HomeBarberSource.City,
                        Label = u.City,
                        Items = list.Take(limit).ToList()
                    };
            }
        }

        // Step 4: No location info — show featured/newest
        var all = await SearchAsync(new BarberSearchDto { RadiusKm = 9999 });
        return new HomeBarberListDto
        {
            Source = HomeBarberSource.Featured,
            Items = all.Take(limit).ToList()
        };
    }

    public async Task<List<BarberListItemDto>> SearchAsync(BarberSearchDto dto)
    {
        var barbers = await _repo.SearchAsync(dto.City);
        var result = new List<BarberListItemDto>();

        foreach (var b in barbers)
        {
            double? distance = null;
            if (dto.Latitude.HasValue && dto.Longitude.HasValue &&
                b.Latitude.HasValue && b.Longitude.HasValue)
            {
                distance = CalculateDistanceKm(
                    dto.Latitude.Value, dto.Longitude.Value,
                    b.Latitude.Value, b.Longitude.Value);

                if (distance > dto.RadiusKm) continue;
            }

            var services = await _repo.GetServicesAsync(b.Id);

            result.Add(new BarberListItemDto
            {
                Id = b.Id,
                ShopName = b.ShopName ?? b.FullName,
                FullName = b.FullName,
                City = b.City,
                Address = b.Address,
                ProfileImageUrl = b.ProfileImageUrl,
                DistanceKm = distance,
                ServiceCount = services.Count,
                MinPrice = services.Any() ? services.Min(s => s.Price) : null
            });
        }

        return result.OrderBy(r => r.DistanceKm ?? double.MaxValue).ToList();
    }

    public async Task<BarberDetailsDto?> GetDetailsAsync(string barberId)
    {
        var barber = await _repo.GetBarberWithDetailsAsync(barberId);
        if (barber is null) return null;

        var leaves = await _repo.GetLeaveDatesAsync(
            barberId,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(60)));

        return new BarberDetailsDto
        {
            Id = barber.Id,
            ShopName = barber.ShopName ?? barber.FullName,
            ShopDescription = barber.ShopDescription,
            FullName = barber.FullName,
            PhoneNumber = barber.PhoneNumber,
            City = barber.City,
            Address = barber.Address,
            ProfileImageUrl = barber.ProfileImageUrl,
            WeeklyHoliday = barber.WeeklyHoliday,
            OpeningTime = barber.OpeningTime,
            ClosingTime = barber.ClosingTime,
            LeaveDates = leaves,
            Services = barber.Services.Select(s => new BarberServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            }).ToList()
        };
    }

    public async Task<AvailableSlotsDto> GetAvailableSlotsAsync(string barberId, DateOnly date, Guid serviceId)
    {
        var barber = await _repo.GetBarberWithDetailsAsync(barberId);
        if (barber is null)
            return new AvailableSlotsDto { Date = date };

        // Check weekly holiday
        if (barber.WeeklyHoliday.HasValue && date.DayOfWeek == barber.WeeklyHoliday.Value)
            return new AvailableSlotsDto { Date = date, IsHoliday = true };

        // Check leave days
        var leaves = await _repo.GetLeaveDatesAsync(barberId, date, date);
        if (leaves.Any())
            return new AvailableSlotsDto { Date = date, IsLeaveDay = true };

        // Default opening hours if not set
        var opening = barber.OpeningTime ?? new TimeOnly(9, 0);
        var closing = barber.ClosingTime ?? new TimeOnly(20, 0);

        var service = barber.Services.FirstOrDefault(s => s.Id == serviceId);
        var duration = service?.DurationMinutes ?? 30;
        var slotInterval = 30;

        var bookings = await _repo.GetBookingsForDateAsync(barberId, date);
        var availableSlots = new List<TimeOnly>();

        var slot = opening;
        var now = DateTime.Now;
        while (slot.AddMinutes(duration) <= closing)
        {
            // skip past slots for today
            if (date == DateOnly.FromDateTime(now) && slot <= TimeOnly.FromDateTime(now))
            {
                slot = slot.AddMinutes(slotInterval);
                continue;
            }

            var slotEnd = slot.AddMinutes(duration);
            var conflict = bookings.Any(b =>
                (slot >= b.StartTime && slot < b.EndTime) ||
                (slotEnd > b.StartTime && slotEnd <= b.EndTime) ||
                (slot <= b.StartTime && slotEnd >= b.EndTime));

            if (!conflict) availableSlots.Add(slot);
            slot = slot.AddMinutes(slotInterval);
        }

        return new AvailableSlotsDto { Date = date, AvailableSlots = availableSlots };
    }

    // Haversine formula
    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);
}
