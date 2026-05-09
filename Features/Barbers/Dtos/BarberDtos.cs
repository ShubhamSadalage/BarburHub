namespace BarberHub.Web.Features.Barbers.Dtos;

public class BarberListItemDto
{
    public string Id { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfileImageUrl { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? DistanceKm { get; set; }
    public int ServiceCount { get; set; }
    public decimal? MinPrice { get; set; }
}

public class BarberDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string? ShopDescription { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DayOfWeek? WeeklyHoliday { get; set; }
    public TimeOnly? OpeningTime { get; set; }
    public TimeOnly? ClosingTime { get; set; }
    public List<BarberServiceDto> Services { get; set; } = new();
    public List<DateOnly> LeaveDates { get; set; } = new();
}

public class BarberServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
}

public class BarberSearchDto
{
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double RadiusKm { get; set; } = 25;
}

public class AvailableSlotsDto
{
    public DateOnly Date { get; set; }
    public List<TimeOnly> AvailableSlots { get; set; } = new();
    public bool IsHoliday { get; set; }
    public bool IsLeaveDay { get; set; }
}
