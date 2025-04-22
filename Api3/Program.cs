using api3.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Configuration;
var builder = WebApplication.CreateBuilder(args);

// 🔹 Register Services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllersWithViews();

// 🔹 Register HttpClient Correctly (if needed)
builder.Services.AddHttpClient<PokemonService>();
builder.Services.AddScoped<PokemonStorageService>();

// 🔹 Register Other Services
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PokemonVentaService>();
builder.Services.AddMemoryCache();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
var app = builder.Build();

// 🔹 Middleware for Error Handling & Security
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ✅ Correct placement for serving static files
app.UseRouting();
app.UseAuthorization(); // ✅ Authorization should be after routing

// 🔹 Configure Routes Correctly
app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pokemon}/{action=Index}/{nombre?}"
);

app.Run();
