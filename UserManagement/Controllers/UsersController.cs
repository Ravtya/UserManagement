using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Filters;
using UserManagement.Models;

namespace UserManagement.Controllers;

[CheckUserFilter]
public class UsersController(AppDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await context.Users
            .OrderByDescending(x => x.LastLogin)
            .Select(x => new UserListItemViewModel
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
                UserStatus = x.UserStatus,
                CreatedAt = x.CreatedAt,
                LastLogin = x.LastLogin
            })
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Block(IEnumerable<int> selectedIds)
    {
        await context.Users.Where(n => selectedIds.Contains(n.Id) && n.UserStatus != UserStatus.Blocked)
            .ExecuteUpdateAsync(n => n.SetProperty(p => p.UserStatus, UserStatus.Blocked));

        HttpContext.Session.SetString("SuccessMessage", "Users blocked successfully");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unblock(IEnumerable<int> selectedIds)
    {
        await context.Users.Where(n => selectedIds.Contains(n.Id) && n.UserStatus == UserStatus.Blocked)
            .ExecuteUpdateAsync(n =>
                n.SetProperty(p => p.UserStatus, p => p.WasVerified ? UserStatus.Active : UserStatus.Unverified));

        HttpContext.Session.SetString("SuccessMessage", "Users unblocked successfully");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(IEnumerable<int> selectedIds)
    {
        await context.Users.Where(n => selectedIds.Contains(n.Id)).ExecuteDeleteAsync();

        HttpContext.Session.SetString("SuccessMessage", "Users deleted successfully");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUnverified()
    {
        await context.Users.Where(n => n.UserStatus == UserStatus.Unverified).ExecuteDeleteAsync();

        HttpContext.Session.SetString("SuccessMessage", "Unverified users deleted successfully");

        return RedirectToAction(nameof(Index));
    }
}