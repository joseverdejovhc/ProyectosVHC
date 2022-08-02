using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nutrisens.Areas.Identity.Data;
using Nutrisens.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(
    options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();



builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();



var app = builder.Build();

// En estas dos líneas guardamos la información de las rutas del servidor donde está la aplicación
AppDomain.CurrentDomain.SetData("ContentRootPath", app.Environment.ContentRootPath);
AppDomain.CurrentDomain.SetData("WebRootPath", app.Environment.WebRootPath);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseDeveloperExceptionPage();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.MapRazorPages();

app.Run();
