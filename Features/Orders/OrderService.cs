using System.ComponentModel.DataAnnotations;
using BarberHub.Web.Domain.Entities;
using BarberHub.Web.Features.Cart;
using BarberHub.Web.Features.Payments;
using BarberHub.Web.Infrastructure.Persistence;
using BarberHub.Web.Shared;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Features.Orders;

public class CheckoutDto
{
    [Required, StringLength(1000)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Online;
}

public class OrderListItemDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public interface IOrderService
{
    /// <summary>
    /// Creates the order. If PaymentMethod is Online, also creates a Stripe PaymentIntent
    /// and returns a clientSecret. If COD, clientSecret is empty and the caller should redirect
    /// straight to the success page.
    /// </summary>
    Task<Result<(Guid orderId, string clientSecret, bool isCod)>> CheckoutAsync(string userId, string email, CheckoutDto dto);
    Task<List<OrderListItemDto>> GetUserOrdersAsync(string userId);
    Task<List<OrderListItemDto>> GetBarberOrdersAsync(string barberId);
    Task<List<OrderListItemDto>> GetMarketplaceOrdersAsync();
    Task<OrderListItemDto?> GetByIdAsync(Guid id);
    Task MarkPaidAsync(string paymentIntentId);

    /// <summary>
    /// Used by the seller (barber for barber-products, SuperAdmin for marketplace) to mark
    /// a Cash-on-Delivery order as paid once they've received cash.
    /// </summary>
    Task<Result> MarkAsPaidByOwnerAsync(Guid orderId, string ownerUserId, bool isSuperAdmin);

    /// <summary>Returns the buyer's UserId for the given order, or null.</summary>
    Task<string?> GetUserIdForOrderAsync(Guid orderId);

    /// <summary>
    /// Notifies the relevant sellers (each barber that owns at least one item, plus
    /// SuperAdmins if the order contains marketplace items) that a new order was placed.
    /// </summary>
    Task NotifyOrderOwnersAsync(Guid orderId);
}

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cart;
    private readonly IStripeService _stripe;
    private readonly Notifications.INotificationService _notifications;
    private readonly Microsoft.AspNetCore.Identity.UserManager<Domain.Entities.ApplicationUser> _userManager;

    public OrderService(
        ApplicationDbContext context,
        ICartService cart,
        IStripeService stripe,
        Notifications.INotificationService notifications,
        Microsoft.AspNetCore.Identity.UserManager<Domain.Entities.ApplicationUser> userManager)
    {
        _context = context;
        _cart = cart;
        _stripe = stripe;
        _notifications = notifications;
        _userManager = userManager;
    }

    public async Task<Result<(Guid orderId, string clientSecret, bool isCod)>> CheckoutAsync(
        string userId, string email, CheckoutDto dto)
    {
        var cart = await _cart.GetAsync(userId);
        if (!cart.Items.Any())
            return Result<(Guid, string, bool)>.Failure("Your cart is empty.");

        // Validate stock
        foreach (var item in cart.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is null || product.StockQuantity < item.Quantity)
                return Result<(Guid, string, bool)>.Failure($"Insufficient stock for {item.ProductName}.");
        }

        var orderNumber = $"BH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            SubTotal = cart.SubTotal,
            Tax = cart.Tax,
            TotalAmount = cart.Total,
            Status = OrderStatus.Pending,
            PaymentMethod = dto.PaymentMethod,
            ShippingAddress = dto.ShippingAddress,
            Items = cart.Items.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                ProductName = c.ProductName,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice
            }).ToList()
        };

        // Decrement stock
        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is not null) product.StockQuantity -= item.Quantity;
        }

        await _context.Orders.AddAsync(order);

        var isCod = dto.PaymentMethod == PaymentMethod.CashOnDelivery;
        var clientSecret = string.Empty;

        if (!isCod)
        {
            // Online payment — create Stripe PaymentIntent
            var intent = await _stripe.CreatePaymentIntentAsync(order.TotalAmount, orderNumber, email);
            order.PaymentIntentId = intent.Id;
            order.PaymentStatus = intent.Status;
            clientSecret = intent.ClientSecret;
        }
        else
        {
            order.PaymentStatus = "cod_pending";
        }

        await _context.SaveChangesAsync();
        await _cart.ClearAsync(userId);

        return Result<(Guid, string, bool)>.Success((order.Id, clientSecret, isCod));
    }

    public async Task<List<OrderListItemDto>> GetUserOrdersAsync(string userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderListItemDto>> GetBarberOrdersAsync(string barberId)
    {
        // Orders that contain at least one product from this barber
        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .Where(o => o.Items.Any(i => i.Product.BarberId == barberId))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderListItemDto>> GetMarketplaceOrdersAsync()
    {
        // Orders that contain at least one marketplace product (BarberId IS NULL)
        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .Where(o => o.Items.Any(i => i.Product.BarberId == null))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderListItemDto?> GetByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? null : MapToDto(order);
    }

    public async Task MarkPaidAsync(string paymentIntentId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.PaymentIntentId == paymentIntentId);
        if (order is null) return;

        order.Status = OrderStatus.Paid;
        order.PaymentStatus = "succeeded";
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<Result> MarkAsPaidByOwnerAsync(Guid orderId, string ownerUserId, bool isSuperAdmin)
    {
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null) return Result.Failure("Order not found.");
        if (order.PaymentMethod != PaymentMethod.CashOnDelivery)
            return Result.Failure("Only Cash-on-Delivery orders can be marked paid manually.");
        if (order.Status == OrderStatus.Paid)
            return Result.Failure("Order is already marked paid.");

        // Authorization: SuperAdmin can mark any order; barber only orders that include their products.
        if (!isSuperAdmin)
        {
            var ownsAny = order.Items.Any(i => i.Product.BarberId == ownerUserId);
            if (!ownsAny) return Result.Failure("You can only mark orders that include your own products.");
        }

        order.Status = OrderStatus.Paid;
        order.PaymentStatus = "cod_paid";
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<string?> GetUserIdForOrderAsync(Guid orderId)
    {
        return await _context.Orders.Where(o => o.Id == orderId).Select(o => o.UserId).FirstOrDefaultAsync();
    }

    public async Task NotifyOrderOwnersAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return;

        var distinctBarbers = order.Items
            .Where(i => !string.IsNullOrEmpty(i.Product.BarberId))
            .Select(i => i.Product.BarberId!)
            .Distinct()
            .ToList();

        var hasMarketplace = order.Items.Any(i => string.IsNullOrEmpty(i.Product.BarberId));
        var buyerName = order.User?.FullName ?? "A customer";

        foreach (var barberId in distinctBarbers)
        {
            await _notifications.NotifyAsync(
                barberId,
                Domain.Entities.NotificationType.OrderPlaced,
                "New order placed",
                $"{buyerName} placed order {order.OrderNumber} ({order.PaymentMethod}).",
                "/Orders/Manage");
        }

        if (hasMarketplace)
        {
            var supers = await _userManager.GetUsersInRoleAsync(Domain.Constants.AppRoles.SuperAdmin);
            foreach (var sa in supers)
            {
                await _notifications.NotifyAsync(
                    sa.Id,
                    Domain.Entities.NotificationType.OrderPlaced,
                    "New marketplace order",
                    $"{buyerName} placed order {order.OrderNumber} ({order.PaymentMethod}).",
                    "/SuperAdmin/Orders");
            }
        }
    }

    private static OrderListItemDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        CreatedAt = o.CreatedAt,
        ItemCount = o.Items.Sum(i => i.Quantity),
        UserName = o.User?.FullName ?? "",
        UserEmail = o.User?.Email ?? "",
        ShippingAddress = o.ShippingAddress,
        Items = o.Items.Select(i => new OrderItemDto
        {
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}
