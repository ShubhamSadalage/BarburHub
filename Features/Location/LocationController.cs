using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarberHub.Web.Features.Location;

public class UpdateLocationDto
{
    [Required, Range(-90, 90)]
    public double Latitude { get; set; }

    [Required, Range(-180, 180)]
    public double Longitude { get; set; }

    [StringLength(200)]
    public string? Label { get; set; }   // optional reverse-geocoded text from client
}

[Authorize]
[Route("[controller]/[action]")]
public class LocationController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;

    public LocationController(UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Called by browser JS after the user grants geolocation permission.
    /// Stores coordinates on the user profile so subsequent visits can use them
    /// even without re-prompting (request 1 in spec: "fall back to previous location").
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] UpdateLocationDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(new { ok = false, error = "Invalid coordinates" });

        var user = await _userManager.FindByIdAsync(_currentUser.UserId!);
        if (user is null) return Unauthorized();

        user.Latitude = dto.Latitude;
        user.Longitude = dto.Longitude;
        user.LocationLabel = dto.Label;
        user.LocationUpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Json(new { ok = true });
    }

    /// <summary>Returns the most recent stored location for this user, if any.</summary>
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId!);
        if (user is null) return Unauthorized();

        return Json(new
        {
            lat = user.Latitude,
            lng = user.Longitude,
            label = user.LocationLabel,
            updatedAt = user.LocationUpdatedAt
        });
    }
}
