namespace BarberHub.Web.Features.Barbers.Dtos;

public enum HomeBarberSource
{
    LiveLocation = 0,    // Browser geolocation
    SavedLocation = 1,   // Previously stored on profile
    City = 2,            // User's city
    Featured = 3         // No location info, fallback
}

public class HomeBarberListDto
{
    public HomeBarberSource Source { get; set; }
    public string? Label { get; set; }              // human-readable, e.g. "Near you", "Pune"
    public List<BarberListItemDto> Items { get; set; } = new();
}
