using api3.Hubs;
using api3.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfiguration>(provider =>
    new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(10);
    options.HandshakeTimeout = TimeSpan.FromMinutes(2); 
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<PokemonService>();

builder.Services.AddScoped<PokemonStorageService>();
builder.Services.AddScoped<CheckoutService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordRecoveryService>();
builder.Services.AddScoped<VentaService>();
builder.Services.AddScoped<ClimaService>();
builder.Services.AddScoped<SolicitudAmistadService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

builder.Services.AddAuthorization();

var app = builder.Build();


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


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();
    endpoints.MapHub<AmistadHub>("/amistadHub");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pokemon}/{action=Index}/{nombre?}"
);


app.Run();
