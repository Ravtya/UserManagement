using System.Security.Claims;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Filters;
using UserManagement.Models;
using UserManagement.Services;

namespace UserManagement.Controllers;

public class AccountController(AppDbContext context, EmailService emailService, ILogger<AccountController> logger)
    : Controller
{
    [HttpGet]
    [RedirectAuthenticated]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RedirectAuthenticated]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == model.Email.ToLowerInvariant());

        if (user is null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        if (user.UserStatus == UserStatus.Blocked)
        {
            ModelState.AddModelError("", "User is blocked");
            return View(model);
        }

        await SignInUser(user);

        return RedirectToAction("Index", "Users");
    }

    [HttpGet]
    [RedirectAuthenticated]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RedirectAuthenticated]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new User
        {
            Email = model.Email.ToLower(),
            Name = model.Name,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.Now,
            UserStatus = UserStatus.Unverified
        };

        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        catch (UniqueConstraintException)
        {
            ModelState.AddModelError("Email", "This  email is already registered");
            return View(model);
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var confirmationLink = emailService.GenerateConfirmationLink(model.Email.ToLowerInvariant(), baseUrl);
                await emailService.SendConfirmationEmailAsync(model.Email.ToLowerInvariant(), model.Name,
                    confirmationLink);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email");
            }
        });

        await SignInUser(user);
        return RedirectToAction("Index", "Users");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token = "")
    {
        var email = emailService.DecodeConfirmationLink(token);

        var user = await context.Users.FirstOrDefaultAsync(n => n.Email == email && n.UserStatus != UserStatus.Blocked);
        if (user != null)
        {
            user.UserStatus = UserStatus.Active;
            await context.SaveChangesAsync();

            HttpContext.Session.SetString("SuccessMessage", "Email confirmed");
        }

        return RedirectToAction("Index", "Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new("status", user.UserStatus.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var dbUser = await context.Users.FindAsync(user.Id);
        if (dbUser != null)
        {
            dbUser.LastLogin = DateTime.Now;
            await context.SaveChangesAsync();
        }

        HttpContext.Session.SetString("SuccessMessage", "Signed in");
    }
}