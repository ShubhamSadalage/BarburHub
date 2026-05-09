using BarberHub.Web.Features.Barbers.Services;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarberHub.Web.Features.Home;

public class HomeController : Controller
{
    private readonly IBarberService _barbers;
    private readonly ICurrentUserService _currentUser;

    public HomeController(IBarberService barbers, ICurrentUserService currentUser)
    {
        _barbers = barbers;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Home. Optional ?lat=&amp;lng=... query string lets the page re-render
    /// with live-location results after the user grants geolocation permission.
    /// </summary>
    public async Task<IActionResult> Index(double? lat = null, double? lng = null)
    {
        var data = await _barbers.GetForHomeAsync(_currentUser.UserId, lat, lng, limit: 12);
        return View(data);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
