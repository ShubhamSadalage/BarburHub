using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Features.Products.Dtos;
using BarberHub.Web.Features.Products.Services;
using BarberHub.Web.Shared.Filters;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberHub.Web.Features.Products.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public ProductsController(
        IProductService service,
        ICurrentUserService currentUser,
        IWebHostEnvironment env)
    {
        _service = service;
        _currentUser = currentUser;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = await _service.GetAllActiveAsync();
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product is null) return NotFound();
        return View(product);
    }

    [HttpGet, Authorize(Roles = AppRoles.Admin), RequireApprovedBarber]
    public async Task<IActionResult> Manage()
    {
        var products = await _service.GetByBarberAsync(_currentUser.UserId!);
        return View(products);
    }

    [HttpGet, Authorize(Roles = AppRoles.Admin), RequireApprovedBarber]
    public IActionResult Create() => View(new CreateProductDto());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin), RequireApprovedBarber]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var imagePath = await SaveImageAsync(dto.Image);
        var result = await _service.CreateAsync(_currentUser.UserId!, dto, imagePath);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(dto);
        }

        TempData["Success"] = "Product created.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpGet, Authorize(Roles = AppRoles.Admin), RequireApprovedBarber]
    public async Task<IActionResult> Edit(Guid id)
    {
        var dto = await _service.GetForEditAsync(_currentUser.UserId!, id);
        if (dto is null) return NotFound();
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin), RequireApprovedBarber]
    public async Task<IActionResult> Edit(EditProductDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var imagePath = await SaveImageAsync(dto.Image);
        var result = await _service.UpdateAsync(_currentUser.UserId!, dto, imagePath);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(dto);
        }

        TempData["Success"] = "Product updated.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin), RequireApprovedBarber]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(_currentUser.UserId!, id);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Product deleted." : result.Error;
        return RedirectToAction(nameof(Manage));
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/products/{fileName}";
    }
}
