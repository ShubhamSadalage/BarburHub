using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberHub.Web.Features.Cart;

[Authorize(Roles = AppRoles.User)]
public class CartController : Controller
{
    private readonly ICartService _service;
    private readonly ICurrentUserService _currentUser;

    public CartController(ICartService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var cart = await _service.GetAsync(_currentUser.UserId!);
        return View(cart);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid productId, int quantity = 1)
    {
        var result = await _service.AddAsync(_currentUser.UserId!, productId, quantity);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Added to cart." : result.Error;
        return RedirectToAction("Index", "Products");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(Guid id, int quantity)
    {
        await _service.UpdateQuantityAsync(_currentUser.UserId!, id, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid id)
    {
        await _service.RemoveAsync(_currentUser.UserId!, id);
        return RedirectToAction(nameof(Index));
    }
}
