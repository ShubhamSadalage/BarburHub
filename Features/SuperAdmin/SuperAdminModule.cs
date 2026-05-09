using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Notifications;
using BarberHub.Web.Features.Orders;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BarberHub.Web.Features.SuperAdmin;

// =================== DTOs ===================

public class PendingBarberDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ShopName { get; set; }
    public string? ShopDescription { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public DateTime? RequestedAt { get; set; }
    public BarberApprovalStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}

public class RejectBarberDto
{
    [Required] public string BarberId { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
}

public class MarketplaceProductDto
{
    public Guid Id { get; set; }

    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [StringLength(2000)] public string? Description { get; set; }

    [Range(0.01, 999999)] public decimal Price { get; set; }
    [Range(0, 100)] public decimal? DiscountPercentage { get; set; }
    [Range(0, 100000)] public int StockQuantity { get; set; }

    [StringLength(100)] public string Category { get; set; } = "General";
    public string? ExistingImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    public bool IsActive { get; set; } = true;
}

// =================== Controller ===================

[Authorize(Roles = AppRoles.SuperAdmin)]
public class SuperAdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IWebHostEnvironment _env;
    private readonly IOrderService _orders;
    private readonly Shared.Services.IAuditService _audit;

    public SuperAdminController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        INotificationService notifications,
        IWebHostEnvironment env,
        IOrderService orders,
        Shared.Services.IAuditService audit)
    {
        _db = db; _userManager = userManager; _currentUser = currentUser;
        _notifications = notifications; _env = env; _orders = orders; _audit = audit;
    }

    // ----------- Dashboard -----------
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var allBarbers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        var allUsers = await _userManager.GetUsersInRoleAsync(AppRoles.User);

        ViewBag.PendingBarbers = allBarbers.Count(b => b.ApprovalStatus == BarberApprovalStatus.Pending);
        ViewBag.ApprovedBarbers = allBarbers.Count(b => b.ApprovalStatus == BarberApprovalStatus.Approved);
        ViewBag.TotalBarbers = allBarbers.Count;
        ViewBag.TotalUsers = allUsers.Count;
        ViewBag.MarketplaceProducts = await _db.Products.CountAsync(p => p.BarberId == null);
        ViewBag.BarberProducts = await _db.Products.CountAsync(p => p.BarberId != null);
        ViewBag.TotalBookings = await _db.Bookings.CountAsync();
        ViewBag.TotalOrders = await _db.Orders.CountAsync();

        return View();
    }

    // ----------- Approvals list -----------
    [HttpGet]
    public async Task<IActionResult> Approvals()
    {
        var barbers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        var dtos = barbers
            .Where(b => b.ApprovalStatus == BarberApprovalStatus.Pending
                        || b.ApprovalStatus == BarberApprovalStatus.Rejected)
            .OrderBy(b => b.ApprovalStatus)
            .ThenByDescending(b => b.ApprovalRequestedAt)
            .Select(b => new PendingBarberDto
            {
                Id = b.Id,
                FullName = b.FullName,
                Email = b.Email ?? "",
                PhoneNumber = b.PhoneNumber,
                ShopName = b.ShopName,
                ShopDescription = b.ShopDescription,
                Address = b.Address,
                City = b.City,
                RequestedAt = b.ApprovalRequestedAt,
                Status = b.ApprovalStatus,
                RejectionReason = b.ApprovalRejectionReason
            }).ToList();

        return View(dtos);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string barberId)
    {
        var barber = await _userManager.FindByIdAsync(barberId);
        if (barber is null)
        {
            TempData["Error"] = "Barber not found.";
            return RedirectToAction(nameof(Approvals));
        }

        barber.ApprovalStatus = BarberApprovalStatus.Approved;
        barber.ApprovedAt = DateTime.UtcNow;
        barber.ApprovedByUserId = _currentUser.UserId;
        barber.ApprovalRejectionReason = null;
        await _userManager.UpdateAsync(barber);

        await _notifications.NotifyAsync(
            barber.Id,
            NotificationType.BarberApproved,
            "Your barber account is approved!",
            "Welcome to Barber Hub. You can now manage your shop, products, and bookings.",
            "/Admin/Dashboard");

        await _audit.LogAsync(User, "BarberApprove", "ApplicationUser", barber.Id,
            $"Email={barber.Email}; Shop={barber.ShopName}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = $"Barber '{barber.FullName}' approved.";
        return RedirectToAction(nameof(Approvals));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(RejectBarberDto dto)
    {
        var barber = await _userManager.FindByIdAsync(dto.BarberId);
        if (barber is null)
        {
            TempData["Error"] = "Barber not found.";
            return RedirectToAction(nameof(Approvals));
        }

        barber.ApprovalStatus = BarberApprovalStatus.Rejected;
        barber.ApprovalRejectionReason = dto.Reason;
        await _userManager.UpdateAsync(barber);

        await _notifications.NotifyAsync(
            barber.Id,
            NotificationType.BarberRejected,
            "Your barber registration was not approved",
            $"Reason: {dto.Reason}",
            "/Auth/PendingApproval");

        await _audit.LogAsync(User, "BarberReject", "ApplicationUser", barber.Id,
            $"Email={barber.Email}; Reason={dto.Reason}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = $"Barber '{barber.FullName}' rejected.";
        return RedirectToAction(nameof(Approvals));
    }

    // ----------- Marketplace Products CRUD -----------
    [HttpGet]
    public async Task<IActionResult> Products()
    {
        var products = await _db.Products
            .Where(p => p.BarberId == null)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(products);
    }

    [HttpGet]
    public IActionResult CreateProduct() => View(new MarketplaceProductDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(MarketplaceProductDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        string? imageUrl = await SaveImageAsync(dto.Image);

        var p = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            DiscountPercentage = dto.DiscountPercentage,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category,
            IsActive = dto.IsActive,
            BarberId = null, // marketplace
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };
        await _db.Products.AddAsync(p);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Marketplace product created.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(Guid id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && x.BarberId == null);
        if (p is null) return NotFound();
        return View(new MarketplaceProductDto
        {
            Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price,
            DiscountPercentage = p.DiscountPercentage, StockQuantity = p.StockQuantity,
            Category = p.Category, ExistingImageUrl = p.ImageUrl, IsActive = p.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(MarketplaceProductDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == dto.Id && x.BarberId == null);
        if (p is null) return NotFound();

        p.Name = dto.Name;
        p.Description = dto.Description;
        p.Price = dto.Price;
        p.DiscountPercentage = dto.DiscountPercentage;
        p.StockQuantity = dto.StockQuantity;
        p.Category = dto.Category;
        p.IsActive = dto.IsActive;
        p.UpdatedAt = DateTime.UtcNow;

        if (dto.Image is { Length: > 0 })
            p.ImageUrl = await SaveImageAsync(dto.Image);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Marketplace product updated.";
        return RedirectToAction(nameof(Products));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && x.BarberId == null);
        if (p is not null)
        {
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Marketplace product removed.";
        }
        return RedirectToAction(nameof(Products));
    }

    // ----------- Marketplace Orders -----------
    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var orders = await _orders.GetMarketplaceOrdersAsync();
        return View(orders);
    }

    // ----------- Manage Users (customers) -----------
    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        var users = await _userManager.GetUsersInRoleAsync(AppRoles.User);
        var list = users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id, u.FullName, u.Email, u.PhoneNumber, u.City, u.IsActive, u.IsDeleted, u.CreatedAt
            }).ToList();
        ViewBag.Users = list;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u is null) { TempData["Error"] = "User not found."; return RedirectToAction(nameof(ManageUsers)); }
        if (u.Id == _currentUser.UserId) { TempData["Error"] = "You cannot delete yourself."; return RedirectToAction(nameof(ManageUsers)); }

        u.IsDeleted = true;
        u.IsActive = false;
        u.DeletedAt = DateTime.UtcNow;
        u.DeletedByUserId = _currentUser.UserId;
        await _userManager.UpdateAsync(u);

        await _audit.LogAsync(User, "UserSoftDelete", "ApplicationUser", u.Id,
            $"Email={u.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = $"User '{u.FullName}' deleted.";
        return RedirectToAction(nameof(ManageUsers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreUser(string id)
    {
        var u = await _userManager.FindByIdAsync(id);
        if (u is null) return NotFound();
        u.IsDeleted = false;
        u.IsActive = true;
        u.DeletedAt = null;
        u.DeletedByUserId = null;
        await _userManager.UpdateAsync(u);
        await _audit.LogAsync(User, "UserRestore", "ApplicationUser", u.Id,
            $"Email={u.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Success"] = $"User '{u.FullName}' restored.";
        return RedirectToAction(nameof(ManageUsers));
    }

    // ----------- Manage Barbers -----------
    [HttpGet]
    public async Task<IActionResult> ManageBarbers()
    {
        var barbers = await _userManager.GetUsersInRoleAsync(AppRoles.Admin);
        ViewBag.Barbers = barbers
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id, b.FullName, b.Email, b.PhoneNumber, b.City, b.ShopName, b.IsActive,
                b.IsDeleted, b.ApprovalStatus, b.CreatedAt
            }).ToList();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBarber(string id)
    {
        var b = await _userManager.FindByIdAsync(id);
        if (b is null) { TempData["Error"] = "Barber not found."; return RedirectToAction(nameof(ManageBarbers)); }

        b.IsDeleted = true;
        b.IsActive = false;
        b.DeletedAt = DateTime.UtcNow;
        b.DeletedByUserId = _currentUser.UserId;
        await _userManager.UpdateAsync(b);

        await _audit.LogAsync(User, "BarberSoftDelete", "ApplicationUser", b.Id,
            $"Email={b.Email}; Shop={b.ShopName}", HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = $"Barber '{b.ShopName ?? b.FullName}' deleted.";
        return RedirectToAction(nameof(ManageBarbers));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreBarber(string id)
    {
        var b = await _userManager.FindByIdAsync(id);
        if (b is null) return NotFound();
        b.IsDeleted = false;
        b.IsActive = true;
        b.DeletedAt = null;
        b.DeletedByUserId = null;
        await _userManager.UpdateAsync(b);
        await _audit.LogAsync(User, "BarberRestore", "ApplicationUser", b.Id,
            $"Email={b.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Success"] = $"Barber '{b.ShopName ?? b.FullName}' restored.";
        return RedirectToAction(nameof(ManageBarbers));
    }

    // ----------- Audit Log viewer -----------
    [HttpGet]
    public async Task<IActionResult> AuditLog(int page = 1)
    {
        const int pageSize = 50;
        var total = await _db.AuditLogs.CountAsync();
        var entries = await _db.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        ViewBag.Page = page;
        ViewBag.HasMore = page * pageSize < total;
        ViewBag.Total = total;
        return View(entries);
    }

    // ----------- Helpers -----------
    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        var ext = Path.GetExtension(file.FileName);
        var fileName = $"marketplace-{Guid.NewGuid()}{ext}";
        var dir = Path.Combine(_env.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/products/{fileName}";
    }
}
