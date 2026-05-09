using BarberHub.Web.Domain.Constants;
using BarberHub.Web.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BarberHub.Web.Shared.Filters;

/// <summary>
/// Apply to admin/barber-only controllers. If the logged-in user is in the Admin (Barber)
/// role but their ApprovalStatus is not Approved, redirect them to the pending page.
/// SuperAdmins always pass through.
/// </summary>
public class RequireApprovedBarberAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.HttpContext.User);

        if (user is null) { await next(); return; }

        // SuperAdmin — always allowed
        if (await userManager.IsInRoleAsync(user, AppRoles.SuperAdmin)) { await next(); return; }

        // Not a barber — let the [Authorize(Roles=…)] attribute on the controller decide
        if (!await userManager.IsInRoleAsync(user, AppRoles.Admin)) { await next(); return; }

        // Barber: must be Approved
        if (user.ApprovalStatus == BarberApprovalStatus.Approved) { await next(); return; }

        // Otherwise, redirect to a friendly status page
        context.Result = new RedirectToActionResult("PendingApproval", "Auth", null);
    }
}
