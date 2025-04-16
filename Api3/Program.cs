using api3.Services;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Register Services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllersWithViews();

// 🔹 Register HttpClient Correctly
builder.Services.AddHttpClient<PokemonService>(); // ✅ Fixed service registration
builder.Services.AddScoped<PokemonStorageService>();

// 🔹 Register Other Services
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PokemonVentaService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

// 🔹 Middleware for Error Handling & Security
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization(); // ✅ Correct placement

// 🔹 Configure Routes Correctly
app.MapControllers();  // ✅ Ensures MVC controllers are mapped correctly
app.MapRazorPages();   // ✅ Ensures Razor Pages routing is supported
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pokemon}/{action=Index}/{nombre?}"
);

app.Run();
