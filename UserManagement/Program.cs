using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using UserManagement;
using UserManagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
        .UseExceptionProcessor());

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

builder.Services.AddSession();
builder.Services.AddDataProtection();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<EmailService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.MapStaticAssets();

app.UseSession();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
        "default",
        "{controller=Users}/{action=Index}")
    .WithStaticAssets();

app.Run();