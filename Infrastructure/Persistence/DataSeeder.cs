using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberHub.Web.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // 1. Seed Roles
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin / Barber user
        var barberEmail = "barber@barberhub.com";
        var barber = await userManager.FindByEmailAsync(barberEmail);
        if (barber is null)
        {
            barber = new ApplicationUser
            {
                UserName = barberEmail,
                Email = barberEmail,
                EmailConfirmed = true,
                PhoneNumber = "9999999999",
                PhoneNumberConfirmed = true,
                FullName = "John's Barber Shop",
                ShopName = "John's Premium Barber Shop",
                ShopDescription = "Premium grooming for modern gentlemen. 15+ years of experience.",
                City = "Pune",
                Address = "123 MG Road, Camp, Pune - 411001",
                Latitude = 18.5204,
                Longitude = 73.8567,
                WeeklyHoliday = DayOfWeek.Monday,
                OpeningTime = new TimeOnly(9, 0),
                ClosingTime = new TimeOnly(20, 0)
            };

            var result = await userManager.CreateAsync(barber, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(barber, AppRoles.Admin);
            }
        }

        // 3. Seed regular user
        var userEmail = "user@barberhub.com";
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                PhoneNumber = "8888888888",
                PhoneNumberConfirmed = true,
                FullName = "Demo User",
                City = "Pune",
                Latitude = 18.5304,
                Longitude = 73.8467
            };
            var result = await userManager.CreateAsync(user, "User@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, AppRoles.User);
            }
        }

        // 4. Seed Services
        if (!await context.Services.AnyAsync())
        {
            var services = new List<Service>
            {
                new() { Name = "Haircut", Description = "Classic men's haircut with styling", Price = 300, DurationMinutes = 30, BarberId = barber!.Id },
                new() { Name = "Beard Trim", Description = "Professional beard trimming and shaping", Price = 200, DurationMinutes = 20, BarberId = barber.Id },
                new() { Name = "Haircut + Beard", Description = "Complete grooming package", Price = 450, DurationMinutes = 45, BarberId = barber.Id },
                new() { Name = "Hair Color", Description = "Hair coloring service", Price = 1200, DurationMinutes = 90, BarberId = barber.Id },
                new() { Name = "Head Massage", Description = "Relaxing head and shoulder massage", Price = 400, DurationMinutes = 30, BarberId = barber.Id }
            };
            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }

        // 5. Seed Products
        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new() { Name = "Premium Hair Wax", Description = "Strong hold hair wax for all-day styling", Price = 450, StockQuantity = 50, Category = "Styling", BarberId = barber!.Id, DiscountPercentage = 10 },
                new() { Name = "Beard Oil", Description = "Natural beard oil for softness and shine", Price = 550, StockQuantity = 40, Category = "Beard Care", BarberId = barber.Id },
                new() { Name = "Hair Shampoo", Description = "Anti-dandruff shampoo 250ml", Price = 320, StockQuantity = 100, Category = "Hair Care", BarberId = barber.Id, DiscountPercentage = 15 },
                new() { Name = "Aftershave Lotion", Description = "Soothing aftershave with menthol", Price = 380, StockQuantity = 60, Category = "Shaving", BarberId = barber.Id },
                new() { Name = "Hair Pomade", Description = "Classic pomade for a sleek look", Price = 650, StockQuantity = 30, Category = "Styling", BarberId = barber.Id },
                new() { Name = "Beard Comb", Description = "Wooden beard comb", Price = 200, StockQuantity = 80, Category = "Accessories", BarberId = barber.Id }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
