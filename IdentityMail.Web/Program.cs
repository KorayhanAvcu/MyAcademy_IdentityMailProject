using IdentityMail.Web.Constants;
using IdentityMail.Web.Context;
using IdentityMail.Web.CustomValidation;
using IdentityMail.Web.Data;
using IdentityMail.Web.Entities;
using IdentityMail.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<CustomErrorDescriber>();
builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.Cookie.Name = "IdentityMailCookie";
});
builder.Services.AddScoped<IEmailService, EmailService>();
var app = builder.Build();

// Identity Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Admin ve User rollerini oluþtur
    //await IdentitySeeder.SeedRolesAsync(services);

    // Rolü olmayan mevcut kullanýcýlarý User yap
    //await IdentitySeeder.SeedExistingUsersAsync(services);

    // Korayhan Avcu'yu Admin yap
    //await IdentitySeeder.SeedAdminAsync(services);
}

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();