using AfroBooks.DataAccess.Data;
using AfroBooks.DataAccess.Repositry;
using AfroBooks.DataAccess.Repositry.IRepositry;
using Microsoft.AspNetCore.Builder;
using AfroBooks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

//// adding IdentityUser as the default identity service for login, register

//builder.Services.AddDefaultIdentity<IdentityUser>(
//// only sign in if the email is confirmed
////options => options.SignIn.RequireConfirmedAccount = true
//).AddEntityFrameworkStores<IdentityApplicationDbContext>();

// discarded after adopting unit of work in our project
//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
//builder.Services.AddRazorPages();

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

// added with identy
app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}"

    );
});


app.Run();
