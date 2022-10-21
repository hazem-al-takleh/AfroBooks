using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry;
using AfroBooks.DataAccess.Repositry.IRepositry;
using Microsoft.AspNetCore.Builder;
using AfroBooks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity.UI.Services;
using AfroBooks.Utility;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// configuring the sql server
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    // finding the connection string to db
    // "DefaultConnection" is the key of the connection kvp
    builder.Configuration.GetConnectionString("DefaultConnection"),
    // debug identity
    b => b.MigrationsAssembly("AfroBooks.DataAccess")
    ));

// commented upon creating a new identity with roles
//////// adding IdentityUser as the default identity service for login, register
////builder.Services.AddDefaultIdentity<IdentityUser>(
////// only sign in if the email is confirmed
//////options => options.SignIn.RequireConfirmedAccount = true
////).AddEntityFrameworkStores<ApplicationDbContext>();
///

//creating a new custom identity with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(
    //options => options.SignIn.RequireConfirmedAccount = true
    )
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

var app = builder.Build();

// HTTP request pipeline: How the app responds to web requests
// The HTTP request passes through different middlewares (the Use)
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

// added with identy
app.UseAuthentication();

app.UseAuthorization();

// added with razor pages of identity
app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area=Customer}/{controller=Home}/{action=Index}"

    );
});


app.Run();
