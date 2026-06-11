using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserManagement.Models;

namespace UserManagement.Filters;

public class CheckUserFilterAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!await CheckIfCurrentUserExistsAndNotBlocked(context))
        {
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "Auth" });
            return;
        }

        await next();
    }

    private static async Task<bool> CheckIfCurrentUserExistsAndNotBlocked(ActionExecutingContext context)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var claims = context.HttpContext.User;

        if (!int.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return false;

        var dbUser = await dbContext.Users.FindAsync(userId);

        return dbUser is not null && dbUser.UserStatus != UserStatus.Blocked;
    }
}