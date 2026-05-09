using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BarberHub.Web.Features.Push;

public class SubscribeDto
{
    [Required] public string Endpoint { get; set; } = string.Empty;
    [Required] public string P256dh { get; set; } = string.Empty;
    [Required] public string Auth { get; set; } = string.Empty;
}

public class UnsubscribeDto
{
    [Required] public string Endpoint { get; set; } = string.Empty;
}

[Authorize]
[Route("[controller]/[action]")]
public class PushController : Controller
{
    private readonly IPushService _push;
    private readonly Shared.Services.ICurrentUserService _currentUser;

    public PushController(IPushService push, Shared.Services.ICurrentUserService currentUser)
    {
        _push = push;
        _currentUser = currentUser;
    }

    /// <summary>Returns the server's VAPID public key so the browser can subscribe.</summary>
    [HttpGet, AllowAnonymous]
    public IActionResult VapidKey()
    {
        var key = _push.GetVapidPublicKey();
        if (string.IsNullOrEmpty(key)) return Json(new { configured = false });
        return Json(new { configured = true, publicKey = key });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe([FromForm] SubscribeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest();
        var ua = Request.Headers.UserAgent.ToString();
        await _push.SaveSubscriptionAsync(_currentUser.UserId!, dto.Endpoint, dto.P256dh, dto.Auth, ua);
        return Json(new { ok = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe([FromForm] UnsubscribeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest();
        await _push.RemoveSubscriptionAsync(dto.Endpoint);
        return Json(new { ok = true });
    }
}
