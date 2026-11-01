using JobLyzerProject_Models.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using JobLyzerProject_Models.Models;
using JobLyzerProject_Services.Interfaces;
using JoblyzerProject_Services.Implementations;
var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection Gateway Pipeline
builder.Services.AddDbContext<JobPlatformContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Custom Security Cookie Authentication Middleware Engine
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Services Dependency Injection
builder.Services.AddScoped<IAuthService, AuthService>();

// TO THIS (which supports both API Controllers and Razor HTML Views):
builder.Services.AddControllersWithViews();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 5. Crucial for mapping assets inside your new 'wwwroot' UI folder (Bootstrap, CSS, JS)
app.UseStaticFiles();

app.UseRouting();

// 6. Security Authentication execution pipelines must sit between UseRouting and UseAuthorization

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// 8. Custom Router mapping logic for HTML Web Pages UI
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();
